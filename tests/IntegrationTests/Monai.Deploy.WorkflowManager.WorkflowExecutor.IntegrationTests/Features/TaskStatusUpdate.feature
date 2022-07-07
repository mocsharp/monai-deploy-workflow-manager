Feature: TaskStatusUpdate

Update task status in the workflow instance from a Task Update Event

@TaskUpdate
Scenario Outline: Publish a valid Task Update event which updates the Task status
    Given I have a clinical workflow Task_Status_Update_Workflow
    And I have a Workflow Instance WFI_Task_Status_Update
    When I publish a Task Update Message Task_Status_Update with status <taskUpdateStatus>
    Then I can see the status of the Task is updated
    Examples:
    | taskUpdateStatus |
    | Accepted         |
    | Succeeded        |
    | Failed           |
    | Canceled         |

@TaskUpdate
Scenario: Publish a valid Task Update event that where WorkflowInstance does not contain TaskId
    Given I have a clinical workflow Task_Status_Update_Workflow
    And I have a Workflow Instance WFI_Task_Status_Update
    When I publish a Task Update Message Task_Status_Update_TaskId_Not_Found with status Succeeded 
    Then I can see the status of the Task is not updated

@TaskUpdate
Scenario Outline: Publish an invalid Task Update event which does not update the task status
    Given I have a clinical workflow Task_Status_Update_Workflow
    And I have a Workflow Instance WFI_Task_Status_Update
    When I publish a Task Update Message <taskUpdateMessage> with status Succeeded 
    Then I can see the status of the Task is not updated
    Examples:
    | taskUpdateMessage                        |
    | Task_Status_Update_Missing_TaskId        |
    | Task_Status_Update_Missing_ExecutionId   |
    | Task_Status_Update_Missing_CorrelationId |
    | Task_Status_Update_Missing_Status        |

@TaskUpdate
Scenario Outline: Publish an valid Task Update event with a status that is invalid for current status
    Given I have a Workflow Instance <existingWFI>
    When I publish a Task Update Message <taskUpdateMessage> with status <taskUpdateStatus>
    Then I can see the status of the Task is not updated
    Examples:
    | existingWFI               | taskUpdateMessage                                | taskUpdateStatus |
    | WFI_Task_Status_Succeeded | Task_Status_Update_Status_Invalid_When_Succeeded | Accepted         |
    | WFI_Task_Status_Failed    | Task_Status_Update_Status_Invalid_When_Failed    | Accepted         |
    | WFI_Task_Status_Canceled  | Task_Status_Update_Status_Invalid_When_Canceled  | Accepted         |

@TaskUpdate
Scenario: Workflow task update test for bucket minio
    Given I have a clinical workflow Workflow_Revision_for_bucket_minio
    And I have a Workflow Instance Workflow_instance_for_bucket_minio
    And I have a payload donkeypayload and bucket in MinIO donkeybucket
    When I publish a Task Update Message Task_status_update_for_bucket_minio with status Succeeded 
    Then I can see the status of the Task is Succeeded

@TaskExport
Scenario: Export task with single destination is in progress, export message is sent 
    Given I have a clinical workflow Workflow_Revision_for_export_single_dest_1
    And I have a Workflow Instance Workflow_Instance_for_export_single_dest_1
    And I have a payload dicom and bucket in MinIO minio
    When I publish a Task Update Message Task_status_update_for_export_single_dest_1 with status Succeeded 
    Then 1 Task Dispatch event is published
    And 1 Export Request message is published

@TaskExport
Scenario: Export task with mutliple destinations is in progress, export message is sent 
    Given I have a clinical workflow Workflow_Revision_for_export_multi_dest_1
    And I have a Workflow Instance Workflow_Instance_for_export_multi_dest_1
    And I have a payload dicom and bucket in MinIO minio
    When I publish a Task Update Message Task_status_update_for_export_multi_dest_1 with status Succeeded 
    Then 1 Task Dispatch event is published
    And 1 Export Request message is published

@TaskExport
Scenario: Export task with single destination and no artifact is in progress, export message is not sent
    Given I have a clinical workflow Workflow_Revision_for_export_single_dest_1
    And I have a Workflow Instance Workflow_Instance_for_export_single_dest_1
    When I publish a Task Update Message Task_status_update_for_export_single_dest_1 with status Succeeded 
    Then 1 Task Dispatch event is published
    And An Export Request message is not published

@TaskExport
Scenario: Export request complete message is sent as Succeeded, next task dispatched
    Given I have a clinical workflow Workflow_Revision_for_export_single_dest_1
    And I have a Workflow Instance Workflow_Instance_for_export_single_dest_1
    And I have a payload dicom and bucket in MinIO minio
    When I publish a Task Update Message Task_status_update_for_export_single_dest_1 with status Succeeded
    And I publish an Export Request message Export_request_for_export_single_dest_1 with status Succeeded
    Then The export request in the worfkflow instance Workflow_Instance_for_export_single_dest_1 is updated to Succeeded
    And 1 Task Dispatch event is published

@TaskExport
Scenario: Export request complete message is sent as Failed, workflow is Failed
    Given I have a clinical workflow Workflow_Revision_for_export_single_dest_1
    And I have a Workflow Instance Workflow_Instance_for_export_single_dest_1
    And I have a payload dicom and bucket in MinIO minio
    When I publish a Task Update Message Task_status_update_for_export_single_dest_1 with status Succeeded
    And I publish an Export Request message Export_request_for_export_single_dest_1 with status Failed
    Then The export request in the worfkflow instance Workflow_Instance_for_export_single_dest_1 is updated to Failed
    And Workflow Instance status is Failed

@TaskExport
Scenario: Export request complete message is sent as Partial Failed, workflow is Failed
    Given I have a clinical workflow Workflow_Revision_for_export_single_dest_1
    And I have a Workflow Instance Workflow_Instance_for_export_single_dest_1
    And I have a payload dicom and bucket in MinIO minio
    When I publish a Task Update Message Task_status_update_for_export_single_dest_1 with status Succeeded
    And I publish an Export Request message Export_request_for_export_single_dest_1 with status Partial Failed
    Then The export request in the worfkflow instance Workflow_Instance_for_export_single_dest_1 is updated to Failed
    And Workflow Instance status is Failed
    



