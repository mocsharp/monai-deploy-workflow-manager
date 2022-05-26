Feature: WorkflowTaskArtefact

Artefacts can get passed into and between tasks

@WorkflowTaskArtefacts
Scenario Outline: Bucket exists in MinIO, publish workflow request which uses input artefacts
    Given I have a bucket in MinIO dicom
    And I have a clinical workflow Single_Task_Context_Input
    When I publish a Workflow Request Message single_task_context_input
    Then I can see task_output_destination Workflow Instance is created
    And task_output_destination Task Dispatch event is published

@WorkflowTaskArtefacts
Scenario Outline: Bucket does not exist in MinIO, publish workflow request which uses non existant bucket
    Given I have a clinical workflow 
    When I publish a Workflow Request Message Bucket_Name
    Then The workflow instance fails

@WorkflowTaskArtefacts
Scenario Outline: Create artefact in MinIO, publish task update message with artefact as output
    Given I have a bucket in MinIO artefact
    And I have a clinical workflow
    And I publish a Workflow Request Message Bucket_Name
    When I publish a task update message output_artefact
    Then The workflow instance is updated with correct file path
    And The task dispatch message is updated with correct file path

@WorkflowTaskArtefacts
Scenario Outline: Bucket exists in MinIO, send task dispatch with non existant file path
    Given I have a bucket in MinIO dicom
    And I have a clinical workflow
    And I have a workflow instance
    When I publish a task dispatch Message non_existant_filepath-required
    Then The workflow instance fails

@WorkflowTaskArtefacts
Scenario Outline: Bucket exists in MinIO, send task dispatch with non existant artefact
    Given I have a bucket in MinIO dicom
    And I have a clinical workflow
    When I publish a workflow instance non_existant_artefact
    Then The workflow instance fails
