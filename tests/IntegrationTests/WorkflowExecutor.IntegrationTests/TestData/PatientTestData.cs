﻿using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.WorkflowExecutor.IntegrationTests.TestData
{
    public class PatientTestData
    {
        public string? Name { get; set; }

        public PatientDetails? Patient { get; set; }
    }

    public static class PatientsTestData
    {
        public static List<PatientTestData> TestData = new List<PatientTestData>()
        {

            new PatientTestData()
            {
                Name = "Full_Patient",
                Patient = new PatientDetails()
                {
                    PatientId = "1",
                    PatientName = "Patient_Full_Patient",
                    PatientSex = "male",
                    PatientDob = new DateTime(2000, 01, 01),
                    PatientAge = "21",
                    PatientHospitalId = "123"
                }
            },
            new PatientTestData()
            {
                Name = "Partial_Patient",
                Patient = new PatientDetails()
                {
                    PatientId = "2",
                    PatientName = "",
                    PatientSex = "female",
                    PatientDob = null,
                    PatientAge = "",
                    PatientHospitalId = ""
                }
            },
            new PatientTestData()
            {
                Name = "Null_Patient",
                Patient = new PatientDetails()
                {
                    PatientId = "",
                    PatientName = "",
                    PatientSex = "",
                    PatientDob = null,
                    PatientAge = "",
                    PatientHospitalId = ""
                }
            },
        };
    }
}
