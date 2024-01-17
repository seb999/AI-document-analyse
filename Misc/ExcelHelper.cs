using System;
using System.IO;
using System.Reflection;
using EwrsDocAnalyses.Class;
using EwrsDocAnalyses.Misc;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

public class ExcelHelper
{
    public static void CreateExcelFile(string filePath, string questionPath)
    {
        // If the file already exists, delete it
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        // create an Excel
        IWorkbook workbook = new XSSFWorkbook();
        ISheet sheet = workbook.CreateSheet("Sheet1");

        // add columns name
        List<Prompt> prompList = JsonHelper.ParseJsonAndGetList(questionPath);
        IRow headerRow = sheet.CreateRow(0);
        headerRow.CreateCell(0).SetCellValue("FileName");
        for (int i = 1; i < prompList.Count; i++)
        {
            headerRow.CreateCell(i).SetCellValue(prompList[i].Key);
        }

        // save the file
        using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            workbook.Write(fs);
        }
    }

    public static void AddMetaDataToExcel(string filePath, MetaData metaData)
    {
        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
        {
            IWorkbook workbook = new XSSFWorkbook(fs);
            ISheet sheet = workbook.GetSheetAt(0);
            int lastRowIndex = sheet.LastRowNum;

            try
            {
                // Create a new row for the metadata
                IRow newRow = sheet.CreateRow(lastRowIndex + 1);

                var headerRow = sheet.GetRow(0);

                for (int i = 0; i < headerRow.Cells.Count; i++)
                {
                    PropertyInfo? propertyInfo = typeof(MetaData).GetProperty(headerRow.Cells[i].StringCellValue);
                    newRow.CreateCell(i).SetCellValue(propertyInfo.GetValue(metaData).ToString());
                }
                // Save the changes
                using (FileStream fs2 = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    workbook.Write(fs2);
                }
            }
            catch (System.Exception)
            {
                Console.WriteLine("VERIFY THAT METADATA CLASS CONTAIN SAME NUMBER OF PROPERTIES AS QUESTIONS");
            }


            // Now, you can set cell values using column names
            // newRow.CreateCell(columnIndexes["Title"]).SetCellValue(metaData.Title);
            // newRow.CreateCell(columnIndexes["Area"]).SetCellValue(metaData.Area);
            // newRow.CreateCell(columnIndexes["Type"]).SetCellValue(metaData.Type);
            // newRow.CreateCell(columnIndexes["Language"]).SetCellValue(metaData.Language);
            // newRow.CreateCell(columnIndexes["Category"]).SetCellValue(metaData.Category);
            // newRow.CreateCell(columnIndexes["Disease"]).SetCellValue(metaData.Disease);
            // newRow.CreateCell(columnIndexes["Tags"]).SetCellValue(metaData.Tags);

            // Set the cell values for the new row
            // newRow.CreateCell(0).SetCellValue(metaData.Title);
            // newRow.CreateCell(1).SetCellValue(metaData.Area);
            // newRow.CreateCell(2).SetCellValue(metaData.Type);
            // newRow.CreateCell(3).SetCellValue(metaData.Language);
            // newRow.CreateCell(4).SetCellValue(metaData.Category);
            // newRow.CreateCell(5).SetCellValue(metaData.Disease);
            // newRow.CreateCell(6).SetCellValue(metaData.Tags);


        }
    }
}
