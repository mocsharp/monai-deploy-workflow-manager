Feature: Test

    Scenario Outline: Connect to MinIO
        Given I have a MinIO spun up
        When I add a bucket
        And I add a file 
        Then I can retrieve the file

