﻿// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Text;
using Ardalis.GuardClauses;
using Argo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common;
using Monai.Deploy.WorkflowManager.Common.Extensions;
using Monai.Deploy.WorkflowManager.ConditionsResolver.Parser;
using Monai.Deploy.WorkflowManager.TaskManager.API;
using Monai.Deploy.WorkflowManager.TaskManager.Argo.Logging;
using Monai.Deploy.WorkflowManager.TaskManager.Argo.StaticValues;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo
{
    public sealed class ArgoPlugin : TaskPluginBase, IAsyncDisposable
    {
        private readonly Dictionary<string, string> _secretStores;
        private readonly Dictionary<string, Messaging.Common.Storage> _intermediaryArtifactStores;
        private readonly IServiceScope _scope;
        private readonly IKubernetesProvider _kubernetesProvider;
        private readonly IArgoProvider _argoProvider;
        private readonly IConditionalParameterParser _conditionalParser;
        private readonly ILogger<ArgoPlugin> _logger;
        private int? _activeDeadlineSeconds;
        private string _namespace;
        private string _baseUrl = null!;
        private string? _apiToken;

        public ArgoPlugin(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<ArgoPlugin> logger,
            TaskDispatchEvent taskDispatchEvent)
            : base(taskDispatchEvent)
        {
            Guard.Against.Null(serviceScopeFactory, nameof(serviceScopeFactory));

            _secretStores = new Dictionary<string, string>();
            _intermediaryArtifactStores = new Dictionary<string, Messaging.Common.Storage>();
            _scope = serviceScopeFactory.CreateScope();

            _kubernetesProvider = _scope.ServiceProvider.GetRequiredService<IKubernetesProvider>() ?? throw new ServiceNotFoundException(nameof(IKubernetesProvider));
            _argoProvider = _scope.ServiceProvider.GetRequiredService<IArgoProvider>() ?? throw new ServiceNotFoundException(nameof(IArgoProvider));
            _conditionalParser = _scope.ServiceProvider.GetRequiredService<IConditionalParameterParser>() ?? throw new ServiceNotFoundException(nameof(IConditionalParameterParser));

            _logger = logger;
            _namespace = Strings.DefaultNamespace;

            ValidateEvent();
            Initialize();
        }

        private void Initialize()
        {
            if (Event.TaskPluginArguments.ContainsKey(Keys.TimeoutSeconds) &&
                int.TryParse(Event.TaskPluginArguments[Keys.TimeoutSeconds], out var result))
            {
                _activeDeadlineSeconds = result;
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.Namespace))
            {
                _namespace = Event.TaskPluginArguments[Keys.Namespace];
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.ArgoApiToken))
            {
                _apiToken = Event.TaskPluginArguments[Keys.ArgoApiToken];
            }

            _baseUrl = Event.TaskPluginArguments[Keys.BaseUrl];

            _logger.Initialized(_namespace, _baseUrl, _activeDeadlineSeconds, (!string.IsNullOrWhiteSpace(_apiToken)));
        }

        private void ValidateEvent()
        {
            if (Event.TaskPluginArguments is null || Event.TaskPluginArguments.Count == 0)
            {
                throw new InvalidTaskException($"Required parameters to execute Argo workflow are missing: {string.Join(',', Keys.RequiredParameters)}");
            }

            foreach (var key in Keys.RequiredParameters)
            {
                if (!Event.TaskPluginArguments.ContainsKey(key))
                {
                    throw new InvalidTaskException($"Required parameter to execute Argo workflow is missing: {key}");
                }
            }

            if (!Uri.IsWellFormedUriString(Event.TaskPluginArguments[Keys.BaseUrl], UriKind.Absolute))
            {
                throw new InvalidTaskException($"The value '{Event.TaskPluginArguments[Keys.BaseUrl]}' provided for '{Keys.BaseUrl}' is not a valid URI.");
            }
        }

        public override async Task<ExecutionStatus> ExecuteTask(CancellationToken cancellationToken = default)
        {
            using var loggerScope = _logger.BeginScope($"Workflow ID={Event.WorkflowInstanceId}, Task ID={Event.TaskId}, Execution ID={Event.ExecutionId}, Argo namespace={_namespace}");

            Workflow workflow;
            try
            {
                workflow = await BuildWorkflowWrapper(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.ErrorGeneratingWorkflow(ex);
                return new ExecutionStatus { Status = TaskExecutionStatus.Failed, FailureReason = FailureReason.PluginError, Errors = ex.Message };
            }

            try
            {
                var client = _argoProvider.CreateClient(_baseUrl, _apiToken);
                _logger.CreatingArgoWorkflow(workflow.Metadata.GenerateName);
                var result = await client.WorkflowService_CreateWorkflowAsync(_namespace, new WorkflowCreateRequest { Namespace = _namespace, Workflow = workflow }, cancellationToken).ConfigureAwait(false);
                _logger.ArgoWorkflowCreated(result.Metadata.Name);
                return new ExecutionStatus { Status = TaskExecutionStatus.Accepted, FailureReason = FailureReason.None };
            }
            catch (Exception ex)
            {
                _logger.ErrorCreatingWorkflow(ex);
                return new ExecutionStatus { Status = TaskExecutionStatus.Failed, FailureReason = FailureReason.PluginError, Errors = ex.Message };
            }
        }

        public override async Task<ExecutionStatus> GetStatus(string identity, CancellationToken cancellationToken = default)
        {
            Guard.Against.NullOrWhiteSpace(identity, nameof(identity));

            try
            {
                var client = _argoProvider.CreateClient(_baseUrl, _apiToken);
                var workflow = await client.WorkflowService_GetWorkflowAsync(_namespace, identity, null, null, cancellationToken).ConfigureAwait(false);

                // it take sometime for the Argo job to be in the final state after emitting the callback event.
                var retryCount = 30;
                while (workflow.Status.Phase.Equals(Strings.ArgoPhaseRunning, StringComparison.OrdinalIgnoreCase) && retryCount-- > 0)
                {
                    await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                    workflow = await client.WorkflowService_GetWorkflowAsync(_namespace, identity, null, null, cancellationToken).ConfigureAwait(false);
                }

                var stats = GetExecutuionStats(workflow);
                if (stats is null)
                {
                    stats = new Dictionary<string, object?>();
                }
                if (Strings.ArgoFailurePhases.Contains(workflow.Status.Phase, StringComparer.OrdinalIgnoreCase))
                {
                    return new ExecutionStatus
                    {
                        Status = TaskExecutionStatus.Failed,
                        FailureReason = FailureReason.ExternalServiceError,
                        Errors = workflow.Status.Message,
                        Stats = stats
                    };
                }
                else if (workflow.Status.Phase.Equals(Strings.ArgoPhaseSucceeded, StringComparison.OrdinalIgnoreCase))
                {
                    return new ExecutionStatus
                    {
                        Status = TaskExecutionStatus.Succeeded,
                        FailureReason = FailureReason.None,
                        Stats = stats
                    };
                }
                else
                {
                    return new ExecutionStatus
                    {
                        Status = TaskExecutionStatus.Unknown,
                        FailureReason = FailureReason.Unknown,
                        Errors = $"Argo status = '{workflow.Status.Phase}'. Messages = '{workflow.Status.Message}'.",
                        Stats = stats
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorCreatingWorkflow(ex);
                return new ExecutionStatus
                {
                    Status = TaskExecutionStatus.Failed,
                    FailureReason = FailureReason.PluginError,
                    Errors = ex.Message
                };
            }
        }

        private Dictionary<string, object?> GetExecutuionStats(Workflow workflow)
        {
            Guard.Against.Null(workflow);

            var stats = new Dictionary<string, object?>
            {
                { "workflowId", Event.WorkflowInstanceId },
                { "duration", workflow.Status?.EstimatedDuration ?? -1 },
                { "resourceDuration", workflow.Status?.ResourcesDuration },
                { "nodeInfo", workflow.Status?.Nodes },
                { "startedAt", workflow.Status?.StartedAt },
                { "finishedAt", workflow.Status?.FinishedAt }
            };

            return stats;
        }

        private async Task<Workflow> BuildWorkflowWrapper(CancellationToken cancellationToken)
        {
            _logger.GeneratingArgoWorkflow();
            var workflow = new Workflow
            {
                ApiVersion = Strings.ArgoApiVersion,
                Kind = Strings.KindWorkflow,
                Metadata = new ObjectMeta()
                {
                    GenerateName = $"md-{Event.TaskPluginArguments![Keys.WorkflowTemplateName]}-",
                    Labels = new Dictionary<string, string>
                    {
                        { Strings.TaskIdLabelSelectorName, Event.TaskId! },
                        { Strings.WorkflowIdLabelSelectorName, Event.WorkflowInstanceId! },
                        { Strings.CorrelationIdLabelSelectorName, Event.CorrelationId! },
                        { Strings.ExecutionIdLabelSelectorName, Event.ExecutionId }
                    }
                },
                Spec = new WorkflowSpec()
                {
                    ActiveDeadlineSeconds = _activeDeadlineSeconds,
                    Entrypoint = Strings.WorkflowEntrypoint,
                    Hooks = new Dictionary<string, LifecycleHook>
                    {
                        { Strings.ExitHook, new LifecycleHook(){ Template = Strings.ExitHookTemplateName }}
                    }
                }
            };

            workflow.Spec.Templates = new List<Template2>();
            // Add the main workflow template
            await AddMainWorkflowTemplate(workflow, cancellationToken).ConfigureAwait(false);

            // Add the exit template for the exit hook
            await AddExitHookTemplate(workflow, cancellationToken).ConfigureAwait(false);

            ProcessTaskPluginArguments(workflow);

            _logger.ArgoWorkflowTemplateGenerated(workflow.Metadata.GenerateName);
            var workflowJson = JsonConvert.SerializeObject(workflow, Formatting.Indented);
            workflowJson = workflowJson.Replace(Event.TaskPluginArguments[Keys.MessagingPassword], "*****");

            _logger.ArgoWorkflowTemplateJson(workflow.Metadata.GenerateName, workflowJson);

            return workflow;
        }

        /// <summary>
        /// Adds limits and requests from task plugin arguments to templates inside given workflow.
        /// </summary>
        /// <param name="resources"></param>
        /// <param name="workflow"></param>
        /// <param name="cancellationToken"></param>
        private void ProcessTaskPluginArguments(Workflow workflow)
        {
            Guard.Against.Null(workflow);

            var resources = Event.GetTaskPluginArgumentsParameter<Dictionary<string, string>>(Keys.ArgoResource);
            var priorityClassName = Event.GetTaskPluginArgumentsParameter(Keys.TaskPriorityClassName);
            var argoParameters = Event.GetTaskPluginArgumentsParameter<Dictionary<string, string>>(Keys.ArgoParameters);

            if (argoParameters is not null)
            {
                foreach (var item in argoParameters)
                {
                    var value = _conditionalParser.ResolveParameters(item.Value, Event.WorkflowInstanceId);
                    workflow.Spec.Arguments.Parameters.Add(new Parameter() { Name = item.Key, Value = value });
                }
            }

            foreach (var template in workflow.Spec.Templates)
            {
                AddLimit(resources, template, ResourcesKeys.CpuLimit);
                AddLimit(resources, template, ResourcesKeys.MemoryLimit);
                AddRequest(resources, template, ResourcesKeys.CpuReservation);
                AddRequest(resources, template, ResourcesKeys.MemoryReservation);
                AddRequest(resources, template, ResourcesKeys.GpuLimit);

                if (priorityClassName is not null)
                {
                    template.PriorityClassName = priorityClassName;
                }
            }
        }

        private static void AddLimit(Dictionary<string, string>? resources, Template2 template, ResourcesKey key)
        {
            Guard.Against.Null(template);
            Guard.Against.Null(key);
            if (template.Container is null || resources is null || !resources.TryGetValue(key.TaskKey, out var value))
            {
                return;
            }
            if (template.Container.Resources is null)
            {
                template.Container.Resources = new ResourceRequirements();
            }
            if (template.Container.Resources.Limits is null)
            {
                template.Container.Resources.Limits = new Dictionary<string, string>();
            }

            template.Container.Resources.Limits.Add(key.ArgoKey, value);
        }

        private static void AddRequest(Dictionary<string, string>? resources, Template2 template, ResourcesKey key)
        {
            Guard.Against.Null(template);
            Guard.Against.Null(key);
            if (template.Container is null || resources is null || !resources.TryGetValue(key.TaskKey, out var value) || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (template.Container.Resources is null)
            {
                template.Container.Resources = new ResourceRequirements();
            }
            if (template.Container.Resources.Requests is null)
            {
                template.Container.Resources.Requests = new Dictionary<string, string>();
            }

            template.Container.Resources.Requests.Add(key.ArgoKey, value);
        }

        private async Task AddMainWorkflowTemplate(Workflow workflow, CancellationToken cancellationToken)
        {
            Guard.Against.Null(workflow, nameof(workflow));

            var workflowTemplate = await LoadWorkflowTemplate(Event.TaskPluginArguments![Keys.WorkflowTemplateName]).ConfigureAwait(false);

            if (workflowTemplate is null)
            {
                throw new TemplateNotFoundException(Event.TaskPluginArguments![Keys.WorkflowTemplateName]);
            }
            var mainTemplateSteps = new Template2()
            {
                Name = Strings.WorkflowEntrypoint,
                Steps = new List<ParallelSteps>()
                {
                    new ParallelSteps() {
                        new WorkflowStep()
                        {
                            Name = Strings.WorkflowEntrypoint,
                            Template = workflowTemplate.Spec.Entrypoint
                        }
                    }
                }
            };

            await CopyWorkflowTemplateToWorkflow(workflowTemplate, workflowTemplate.Spec.Entrypoint, workflow, cancellationToken).ConfigureAwait(false);
            workflow.Spec.Templates.Add(mainTemplateSteps);

            await ConfigureInputArtifactStoreForTemplates(workflow.Spec.Templates, cancellationToken).ConfigureAwait(false);
            await ConfigureOuputArtifactStoreForTemplates(workflow.Spec.Templates, cancellationToken).ConfigureAwait(false);
        }

        private async Task AddExitHookTemplate(Workflow workflow, CancellationToken cancellationToken)
        {
            Guard.Against.Null(workflow, nameof(workflow));

            var temporaryStore = Event.IntermediateStorage.Clone() as Messaging.Common.Storage;
            temporaryStore!.RelativeRootPath = $"{temporaryStore.RelativeRootPath}/{{{{ workflow.name }}}}/messaging";

            var exitTemplateSteps = new Template2()
            {
                Name = Strings.ExitHookTemplateName,
                Steps = new List<ParallelSteps>()
                {
                    new ParallelSteps()
                    {
                        new WorkflowStep()
                        {
                            Name = Strings.ExitHookTemplateGenerateTemplateName,
                            Template = Strings.ExitHookTemplateGenerateTemplateName,
                        }
                    },

                    new ParallelSteps()
                    {
                        new WorkflowStep()
                        {
                            Name = Strings.ExitHookTemplateSendTemplateName,
                            Template = Strings.ExitHookTemplateSendTemplateName,
                        }
                    }
                }
            };

            workflow.Spec.Templates.Add(exitTemplateSteps);

            var artifact = await CreateArtifact(temporaryStore, cancellationToken).ConfigureAwait(false);

            var exitHookTemplate = new ExitHookTemplate(Event);
            workflow.Spec.Templates.Add(exitHookTemplate.GenerateMessageTemplate(artifact));
            workflow.Spec.Templates.Add(exitHookTemplate.GenerateSendTemplate(artifact));
        }

        private async Task<WorkflowTemplate> LoadWorkflowTemplate(string workflowTemplateName)
        {
            Guard.Against.NullOrWhiteSpace(workflowTemplateName, nameof(workflowTemplateName));

            try
            {
                var client = _argoProvider.CreateClient(_baseUrl, _apiToken);
                return await client.WorkflowTemplateService_GetWorkflowTemplateAsync(_namespace, workflowTemplateName, null).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.ErrorLoadingWorkflowTemplate(workflowTemplateName, ex);
                throw;
            }
        }

        private async Task CopyWorkflowTemplateToWorkflow(WorkflowTemplate workflowTemplate, string name, Workflow workflow, CancellationToken cancellationToken)
        {
            Guard.Against.Null(workflowTemplate, nameof(workflowTemplate));
            Guard.Against.Null(workflowTemplate.Spec, nameof(workflowTemplate.Spec));
            Guard.Against.Null(workflowTemplate.Spec.Templates, nameof(workflowTemplate.Spec.Templates));
            Guard.Against.NullOrWhiteSpace(name, nameof(name));
            Guard.Against.Null(workflow, nameof(workflow));

            var template = workflowTemplate.Spec.Templates.FirstOrDefault(p => p.Name == name);
            if (template is null)
            {
                throw new TemplateNotFoundException(workflowTemplate.Metadata.Name, name);
            }

            workflow.Spec.Templates.Add(template);

            await CopyTemplateSteps(template.Steps, workflowTemplate, name, workflow, cancellationToken).ConfigureAwait(false);
            await CopyTemplateDags(template.Dag, workflowTemplate, name, workflow, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Configures input artifact store for all templates.
        /// For dag & steps, if an argument with S3 Key is set to <see cref="Strings.InputRepositoryToken"/>
        /// with a matching name in <see cref="TaskDispatchEvent.Inputs"/>, then the connection information is used.
        /// For all other template types, if a matching name in <see cref="TaskDispatchEvent.Inputs"/>, then the connection information is used.
        /// Otherwise, the ArgoPlugin assume that a connection is provided.
        /// </summary>
        /// <param name="templates"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ConfigureInputArtifactStoreForTemplates(ICollection<Template2> templates, CancellationToken cancellationToken)
        {
            Guard.Against.Null(templates, nameof(templates));

            foreach (var template in templates)
            {
                if (template.Dag is not null)
                {
                    await ConfigureInputArtifactStore(template.Name, templates, template.Dag.Tasks.Where(p => p.Arguments is not null).SelectMany(p => p.Arguments.Artifacts), true, cancellationToken).ConfigureAwait(false);
                }
                else if (template.Steps is not null)
                {
                    foreach (var step in template.Steps)
                    {
                        await ConfigureInputArtifactStore(template.Name, templates, step.Where(p => p.Arguments is not null).SelectMany(p => p.Arguments.Artifacts), true, cancellationToken).ConfigureAwait(false);
                    }
                }
                else if (template.Inputs is not null)
                {
                    await ConfigureInputArtifactStore(template.Name, templates, template.Inputs.Artifacts, false, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private async Task ConfigureInputArtifactStore(string templateName, ICollection<Template2> templates, IEnumerable<Artifact> artifacts, bool isDagOrStep, CancellationToken cancellationToken)
        {
            Guard.Against.NullOrWhiteSpace(templateName, nameof(templateName));
            Guard.Against.Null(templates, nameof(templates));

            if (artifacts is null || !artifacts.Any())
            {
                return;
            }

            foreach (var artifact in artifacts)
            {
                if (!isDagOrStep && IsInputConfiguredInStepOrDag(templates, templateName, artifact.Name))
                {
                    continue;
                }

                var storageInfo = Event.Inputs.FirstOrDefault(p => p.Name.Equals(artifact.Name, StringComparison.Ordinal));
                if (storageInfo is null)
                {
                    _logger.NoInputArtifactStorageConfigured(artifact.Name, templateName);
                    return;
                }
                artifact.S3 = await CreateArtifact(storageInfo, cancellationToken).ConfigureAwait(false);
            }
        }

        private bool IsInputConfiguredInStepOrDag(ICollection<Template2> templates, string referencedTemplateName, string referencedArtifactName)
        {
            Guard.Against.Null(templates, nameof(templates));
            Guard.Against.NullOrWhiteSpace(referencedTemplateName, nameof(referencedTemplateName));
            Guard.Against.NullOrWhiteSpace(referencedArtifactName, nameof(referencedArtifactName));

            List<Artifact> artifacts = new List<Artifact>();
            foreach (var template in templates)
            {
                if (template.Dag is not null)
                {
                    artifacts.AddRange(template.Dag.Tasks
                        .Where(p => p.Template.Equals(referencedTemplateName, StringComparison.Ordinal) && p.Arguments is not null)
                        .SelectMany(p => p.Arguments.Artifacts));
                }
                else if (template.Steps is not null)
                {
                    foreach (var step in template.Steps)
                    {
                        artifacts.AddRange(step.Where(p => p.Template.Equals(referencedTemplateName, StringComparison.OrdinalIgnoreCase) && p.Arguments is not null)
                           .SelectMany(p => p.Arguments.Artifacts));
                    }
                }
            }

            return artifacts.Any(p => p.Name.Equals(referencedArtifactName, StringComparison.Ordinal));
        }

        /// <summary>
        /// Configures output artifact store for non-dag & non-steps templates.
        /// If a matching output name in the template is found in <see cref="TaskDispatchEvent.Outputs"/>, the connection information is used and is assumed to be a task output.
        /// Otherwise, the <see cref="TaskDispatchEvent.IntermediateStorage"/> output store is used and a subdirectory is created & mapped into the container.
        /// </summary>
        /// <param name="templates"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ConfigureOuputArtifactStoreForTemplates(ICollection<Template2> templates, CancellationToken cancellationToken)
        {
            Guard.Against.Null(templates, nameof(templates));

            foreach (var template in templates)
            {
                if (template.Dag is not null || template.Steps is not null)
                {
                    continue;
                }

                await SetupOutputArtifactStoreForTemplate(template, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task SetupOutputArtifactStoreForTemplate(Template2 template, CancellationToken cancellationToken)
        {
            Guard.Against.Null(template, nameof(template));

            if (template.Outputs is null || template.Outputs.Artifacts is null)
            {
                return;
            }

            foreach (var artifact in template.Outputs.Artifacts)
            {
                var storageInfo = Event.Outputs.FirstOrDefault(p => p.Name.Equals(artifact.Name, StringComparison.Ordinal));

                if (storageInfo is null)
                {
                    storageInfo = GenerateIntermediaryArtifactStore(artifact.Name);
                    _logger.UseIntermediaryArtifactStorage(artifact.Name, template.Name);
                }

                artifact.S3 = await CreateArtifact(storageInfo, cancellationToken).ConfigureAwait(false);
                artifact.Archive = new ArchiveStrategy
                {
                    None = new NoneStrategy()
                };
            }
        }

        private Messaging.Common.Storage GenerateIntermediaryArtifactStore(string artifactName)
        {
            if (_intermediaryArtifactStores.ContainsKey(artifactName))
            {
                return _intermediaryArtifactStores[artifactName];
            }

            var storageInfo = Event.IntermediateStorage.Clone() as Messaging.Common.Storage;
            storageInfo!.RelativeRootPath = $"{storageInfo.RelativeRootPath}/{{{{ workflow.name }}}}/{artifactName}";

            _intermediaryArtifactStores.Add(artifactName, storageInfo);

            return storageInfo;
        }

        private async Task<S3Artifact2> CreateArtifact(Messaging.Common.Storage storageInfo, CancellationToken cancellationToken)
        {
            Guard.Against.Null(storageInfo, nameof(storageInfo));

            if (!_secretStores.TryGetValue(storageInfo.Name!, out var secret))
            {
                secret = await GenerateK8sSecretFrom(storageInfo, cancellationToken).ConfigureAwait(false);
            }

            return new S3Artifact2()
            {
                Bucket = storageInfo.Bucket,
                Key = storageInfo.RelativeRootPath,
                Insecure = !storageInfo.SecuredConnection,
                Endpoint = storageInfo.Endpoint,
                AccessKeySecret = new SecretKeySelector { Name = secret, Key = Strings.SecretAccessKey },
                SecretKeySecret = new SecretKeySelector { Name = secret, Key = Strings.SecretSecretKey },
            };
        }

        private async Task CopyTemplateDags(DAGTemplate dag, WorkflowTemplate workflowTemplate, string name, Workflow workflow, CancellationToken cancellationToken)
        {
            Guard.Against.Null(workflowTemplate, nameof(workflowTemplate));
            Guard.Against.NullOrWhiteSpace(name, nameof(name));
            Guard.Against.Null(workflow, nameof(workflow));

            if (dag is not null)
            {
                foreach (var task in dag.Tasks)
                {
                    await CopyWorkflowTemplateToWorkflow(workflowTemplate, task.Template, workflow, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private async Task CopyTemplateSteps(ICollection<ParallelSteps> steps, WorkflowTemplate workflowTemplate, string name, Workflow workflow, CancellationToken cancellationToken)
        {
            Guard.Against.Null(workflowTemplate, nameof(workflowTemplate));
            Guard.Against.NullOrWhiteSpace(name, nameof(name));
            Guard.Against.Null(workflow, nameof(workflow));

            if (steps is not null)
            {
                foreach (var pStep in steps)
                {
                    foreach (var step in pStep)
                    {
                        await CopyWorkflowTemplateToWorkflow(workflowTemplate, step.Template, workflow, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task<string> GenerateK8sSecretFrom(Messaging.Common.Storage storage, CancellationToken cancellationToken)
        {
            Guard.Against.Null(storage, nameof(storage));
            Guard.Against.Null(storage.Credentials, nameof(storage.Credentials));
            Guard.Against.NullOrWhiteSpace(storage.Name, nameof(storage.Name));
            Guard.Against.NullOrWhiteSpace(storage.Credentials.AccessKey, nameof(storage.Credentials.AccessKey));
            Guard.Against.NullOrWhiteSpace(storage.Credentials.AccessToken, nameof(storage.Credentials.AccessToken));

            var client = _kubernetesProvider.CreateClient();
            var secret = new k8s.Models.V1Secret
            {
                Metadata = new k8s.Models.V1ObjectMeta
                {
                    Name = $"{storage.Name.ToLowerInvariant()}-{DateTime.UtcNow.Ticks}",
                    Labels = new Dictionary<string, string>
                    {
                        { Strings.LabelCreator, Strings.LabelCreatorValue }
                    }
                },
                Type = Strings.SecretTypeOpaque,
                Data = new Dictionary<string, byte[]>
                {
                    { Strings.SecretAccessKey, Encoding.UTF8.GetBytes(storage.Credentials.AccessKey) },
                    { Strings.SecretSecretKey, Encoding.UTF8.GetBytes(storage.Credentials.AccessToken) }
                }
            };

            _logger.GeneratingArtifactSecret(storage.Name);
            var result = await client.CreateNamespacedSecretWithHttpMessagesAsync(secret, _namespace, cancellationToken: cancellationToken).ConfigureAwait(false);
            result.Response.EnsureSuccessStatusCode();
            _secretStores.Add(storage.Name, result.Body.Metadata.Name);
            return result.Body.Metadata.Name;
        }

        private async Task RemoveKubenetesSecrets()
        {
            if (_secretStores.Any())
            {
                var client = _kubernetesProvider.CreateClient();

                foreach (var secret in _secretStores.Values)
                {
                    try
                    {
                        await client.DeleteNamespacedSecretWithHttpMessagesAsync(secret, _namespace).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.ErrorDeletingKubernetesSecret(secret, ex);
                    }
                }
                _secretStores.Clear();
            }
        }

        ~ArgoPlugin() => Dispose(disposing: false);

        protected override void Dispose(bool disposing)
        {
            if (!DisposedValue && disposing)
            {
                _scope.Dispose();
            }

            base.Dispose(disposing);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);

            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        private async ValueTask DisposeAsyncCore()
        {
            await RemoveKubenetesSecrets().ConfigureAwait(false);
        }
    }
}
