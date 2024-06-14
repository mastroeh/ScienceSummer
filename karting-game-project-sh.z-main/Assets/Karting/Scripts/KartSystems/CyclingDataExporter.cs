using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

public class CyclingDataExporter : MonoBehaviour
{
    private List<DataEntry> dataEntries = new List<DataEntry>();
    private float interval = 5.0f; // The time interval is set to 5 seconds
    private FTMS_IndoorBike ftms_bike;
    // Reference BikeInput instance
    public BikeInput bikeInput;

    private void Start()
    {
        ftms_bike = FTMS_IndoorBike.Instance;
        InvokeRepeating("CollectData", 0f, interval);
    }

    private void CollectData()
    {
        if (ftms_bike != null && bikeInput != null) { 
            float currentRPM = ftms_bike.rpm;
            float currentPower = ftms_bike.power;

            AddDataToList(currentRPM, currentPower, bikeInput.leftTurnPressCount, bikeInput.rightTurnPressCount, bikeInput.maxLeftTurnDuration, bikeInput.minLeftTurnDuration,
                          bikeInput.maxRightTurnDuration, bikeInput.minRightTurnDuration,
                          bikeInput.bodyTurnCount, bikeInput.maxBodyTurnInput, bikeInput.minBodyTurnInput);

            //// Reset relevant statistics in BikeInput
            //bikeInput.maxLeftTurnDuration = 0f;
            //bikeInput.minLeftTurnDuration = float.MaxValue;
            //bikeInput.maxRightTurnDuration = 0f;
            //bikeInput.minRightTurnDuration = float.MaxValue;
            //bikeInput.bodyTurnCount = 0;
            //bikeInput.maxBodyTurnInput = float.MinValue;
            //bikeInput.minBodyTurnInput = float.MaxValue;
        }
    }

    private void AddDataToList(float rpm, float power, int leftTurnPressCount, int rightTurnPressCount,
                               float maxLeftDuration, float minLeftDuration,
                               float maxRightDuration, float minRightDuration,
                               int bodyTurns, float maxBodyTurn, float minBodyTurn)
    {
        dataEntries.Add(new DataEntry()
        {
            RPM = rpm,
            //AverageRPM = averageRpm,
            Power = power,
            //AveragePower = averagePower,
            Timestamp = DateTime.Now,
            LeftButtonPressedCount = leftTurnPressCount,
            RightButtonPressedCount = rightTurnPressCount,
            MaxLeftTurnDuration = maxLeftDuration,
            MinLeftTurnDuration = minLeftDuration,
            MaxRightTurnDuration = maxRightDuration,
            MinRightTurnDuration = minRightDuration,
            BodyTurnCount = bodyTurns,
            MaxBodyTurnInput = maxBodyTurn,
            MinBodyTurnInput = minBodyTurn
        });
    }

    private void OnApplicationQuit()
    {
        ExportDataToCSV();
    }

    private void ExportDataToCSV()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Timestamp,RPM,Power,LeftButtonPressedCount, RightButtonPressedCount, MaxLeftTurnDuration,MinLeftTurnDuration,MaxRightTurnDuration,MinRightTurnDuration,BodyTurnCount,MaxBodyTurnInput,MinBodyTurnInput");

        foreach (var entry in dataEntries)
        {
            sb.AppendLine($"{entry.Timestamp},{entry.RPM},{entry.Power},{entry.LeftButtonPressedCount},{entry.RightButtonPressedCount},{entry.MaxLeftTurnDuration},{entry.MinLeftTurnDuration},{entry.MaxRightTurnDuration},{entry.MinRightTurnDuration},{entry.BodyTurnCount},{entry.MaxBodyTurnInput},{entry.MinBodyTurnInput}");
        }

        string filePath = GetFilePath();
        File.WriteAllText(filePath, sb.ToString());
        Debug.Log("Data exported to: " + filePath);
    }

    private string GetFilePath()
    {
        string folderPath = @"D:\Shuoheng Zhang\10-LogData";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string fileName = $"CyclingData_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        return Path.Combine(folderPath, fileName);
    }

    class DataEntry
    {
        public int LeftButtonPressedCount;
        public float MaxLeftTurnDuration;
        public float MinLeftTurnDuration;

        public int RightButtonPressedCount;
        public float MaxRightTurnDuration;
        public float MinRightTurnDuration;

        public int BodyTurnCount;
        public float MaxBodyTurnInput;
        public float MinBodyTurnInput;
        public float RPM;
        public float Power;
        public DateTime Timestamp;
    }
}
