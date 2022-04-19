Feature: Test

    Scenario Outline: Basic mongo and rabbit test
        Given I have a DAG in Mongo Dag_Mongo_Connection
        When I publish an Export Message Request ExportMessageRequest_1
        Then I can see the event ExportMessageRequest_1
        And I can retrieve the DAG Dag_Mongo_Connection
        Examples:
            | WorkflowMessage        |
            | ExportMessageRequest_1 |
            | ExportMessageRequest_2 |

    Scenario Outline: Connect to MinIO
        Given I have a MinIO spun up
        When I add a file 
        Then I can see the file
        And I can retrieve the file

