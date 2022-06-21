Feature: Example

A short summary of the feature

@tag1
Scenario: Create bucket and publish task dispatch message
	Given A study is uploaded to the storage service
	When A Task Dispatch event is published
	Then I can see the event is consumed
