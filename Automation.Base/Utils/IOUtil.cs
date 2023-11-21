using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace CommonSpirit.Automation.Base.Utils
{
    class IOUtil
    {
		public static Dictionary<string, string> dataDic = new Dictionary<string, string>();
		public static string iterationDataPath = string.Empty;

		public static Dictionary<string, string> GetInputData(ISheet sheet, string testCaseName, int intCurrentIteration)
        {
            try
            {
				//Open the excel sheet and fetch all the common parameters
				IRow rowHeader, row;
				FetchCommonParameters(sheet);
				
				//Store value in dictionary
				var rowNum = FindRowNumber(sheet, testCaseName);
				dataDic.Add("TC_Name", testCaseName);

				//If no test data found for the test case, then return dictionary with TC_Name and Description
				if (rowNum == -1)
				{
					dataDic.Add("Description", "No Description found");
					return dataDic;
				}

				dataDic.Add("Description", sheet.GetRow(rowNum).GetCell(1).StringCellValue);
                rowHeader = sheet.GetRow(rowNum - 1);
				
				for (var i = rowNum; ; i++)
				{
					if (sheet.GetRow(i).GetCell(2).StringCellValue.Equals("Iteration" + intCurrentIteration, StringComparison.OrdinalIgnoreCase))
					{
						row = sheet.GetRow(i);
						dataDic.Add("Iteration", sheet.GetRow(i).GetCell(2).StringCellValue);
						break;
					}
				}

				FetchRowData(row, rowHeader, 3);
            }
            catch (Exception) { }

            return dataDic;
        }

		public static void FetchCommonParameters(ISheet sheet)
		{
			try
			{
				//Open the excel sheet
				dataDic.Clear();
				var rowNum = FindRowNumber(sheet, "Common Parameters");
				var rowHeader = sheet.GetRow(rowNum - 1);
				var row = sheet.GetRow(rowNum);
				FetchRowData(row, rowHeader, 1);
			}
			catch(Exception) { }
		}

		public static void FetchRowData(IRow row, IRow rowHeader, int startIndex)
		{
			var df = new DataFormatter();

			for (var i = startIndex; i < row.LastCellNum; i++)
			{
				var currentCell = row.GetCell(i);
				if (currentCell == null) { continue; }

				switch (currentCell.CellType)
				{
					case CellType.String:
						dataDic.Remove(rowHeader.GetCell(i).StringCellValue);
						dataDic.Add(rowHeader.GetCell(i).StringCellValue, currentCell.StringCellValue);
						break;
					case CellType.Numeric:
						dataDic.Remove(rowHeader.GetCell(i).StringCellValue);
						dataDic.Add(rowHeader.GetCell(i).StringCellValue, df.FormatCellValue(currentCell));
						break;
					case CellType.Blank:
						dataDic.Remove(rowHeader.GetCell(i).StringCellValue);
						dataDic.Add(rowHeader.GetCell(i).StringCellValue, "");
						break;
				}
			}
		}

        public static int GetIterationCountExcel(string testCaseName)
        {
            var size = 1;
            try
            {
				//Store value in dictionary
				var df = new DataFormatter();
				var sheet = GetDataSheet(iterationDataPath);
				var rowNum = FindRowNumber(sheet, testCaseName);

                if (rowNum != -1)
                {
                    for (var i = rowNum + 1; ; i++)
                    {
                        try
                        {
                            if (df.FormatCellValue(sheet.GetRow(i).GetCell(2)).ToUpper().StartsWith("ITERATION"))
							{
								size += 1;
							}
							else
							{
								break;
							}
						}
                        catch (Exception) { break; }
                    }
                }
            }
            catch (Exception) { }

            return size;
        }

        private static int FindRowNumber(ISheet sheet, string testCaseName)
        {
            var rowNum = -1;
            var df = new DataFormatter();

            for (var i = 0; i <= sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);

				try
				{
					var cell = row.GetCell(0);

					if (Equals(testCaseName.ToUpper(), df.FormatCellValue(cell).ToUpper()))
					{
						rowNum = i;
						break;
					}
				}
				catch (Exception) { }
            }

            return rowNum;
		}

		public static ISheet GetDataSheet(string inputFilepath)
		{
			ISheet currentSheet = null;

			try
			{
				using var fileStream = new FileStream(inputFilepath, FileMode.Open, FileAccess.Read);
				var workBook = new XSSFWorkbook(fileStream);
				currentSheet = workBook.GetSheetAt(0);
				workBook.Close();
			}
			catch (Exception) { }

			return currentSheet;
		}

		public static void SetIterationList(ISheet sheet)
		{
			try
			{
				//Store value in dictionary
				var df = new DataFormatter();

				for (var i = 0; i <= sheet.LastRowNum; i++)
				{
					var row = sheet.GetRow(i);

					try
					{
						var cell = row.GetCell(0);
						var size = 0;

						if (df.FormatCellValue(cell).ToUpper().Equals("TC_NAME") || string.IsNullOrWhiteSpace(df.FormatCellValue(cell)))
						{
							continue;
						}

						for (var j = i; ; j++)
						{
							try
							{
								if (df.FormatCellValue(sheet.GetRow(j).GetCell(2)).ToUpper().StartsWith("ITERATION"))
								{
									size += 1;
								}
								else
								{
									break;
								}
							}
							catch (Exception) { break; }
						}

						CommonUtilities.testIterationCount.Add(df.FormatCellValue(cell), size);
						CommonUtilities.currentTestIteration.Add(df.FormatCellValue(cell), 1);
					}
					catch (Exception) { }
				}
			}
			catch (Exception) { }
		}
	}
}