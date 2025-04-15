using ClosedXML.Excel;

using DocumentFormat.OpenXml.Office2016.Drawing.Charts;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peloton_IDE.Presentation
{
    public sealed partial class TranslatePage : Microsoft.UI.Xaml.Controls.Page
    {
        private (bool dictOk, SortedDictionary<string, (double _typeCode, string _text)> dict) FillSortedDictionaryFromWorksheet(SortedDictionary<string, (double _typeCode, string _text)> sortedDictionary, IXLWorksheet? worksheet, int sourceCol, int targetCol)
        {
            IXLRows rows = worksheet.Rows();
            for (int i = 2; i <= rows.Count(); i++)
            {
                IXLCell typeCodeCell = worksheet.Cell(i, 1);
                string typeCodeString = typeCodeCell.GetString().Trim();
                double typeCode = 11;
                if (typeCodeString != "")
                {
                    typeCode = double.Parse(typeCodeString);
                }
                IXLCell sourceCell = worksheet.Cell(i, sourceCol + 1);
                string sourceText = sourceCell.GetString().Trim();
                IXLCell targetCell = worksheet.Cell(i, targetCol + 1);
                string targetText = targetCell.GetString();
                if (sourceText.Length > 0 && targetText.Length > 0)
                    sortedDictionary[sourceText] = (typeCode, targetText); // smartness: 0
            }
            if (sortedDictionary.Count == 0) return (false, sortedDictionary);
            return (true, sortedDictionary);
        }
        private (bool stOk, int sourceCol, int targetCol) GetSourceAndTargetColumnsFromWorksheet(IXLWorksheet? worksheet, long sourceLanguageId, long targetLanguageId)
        {
            // find column named after name of target language
            // find column named after name of source language
            int sourceCol = -1;
            int targetCol = -1;
            string sourceTag = $"[{sourceLanguageId}]";
            string targetTag = $"[{targetLanguageId}]";
            IXLColumns columns = worksheet.Columns();
            for (int i = 0; i < columns.Count(); i++)
            {
                IXLColumn column = columns.ElementAt(i);
                IXLCell head = column.Cell(1);
                if (head.GetString().StartsWith(sourceTag))
                {
                    sourceCol = i;
                }
                if (head.GetString().StartsWith(targetTag))
                {
                    targetCol = i;
                }
                if (sourceCol > -1 && targetCol > -1) break;
            }

            // if !found, end
            if (sourceCol == -1 || targetCol == -1) return (false, sourceCol, targetCol);
            return (true, sourceCol, targetCol);
        }
        private (bool wsOk, IXLWorksheet? xLWorksheet) GetNamedWorksheetInExcelWorkbook(XLWorkbook? workbook, string? nameOfSource, string? nameOfDefault)
        {
            if (workbook.Worksheets.Contains(nameOfSource))
            {
                return (true,  workbook.Worksheet(nameOfSource));
            }
            else
            {
                if (workbook.Worksheets.Contains(nameOfDefault))
                {
                    return (true, workbook.Worksheet(nameOfDefault));
                }
                else
                {
                    return (false,null);
                }
            }
        }
        private (bool ok, XLWorkbook? workbook) GetNamedExcelWorkbook(string? xlsxPath)
        {
            if (!File.Exists(xlsxPath)) return (false, null);
            using FileStream xlsxStream = File.Open(xlsxPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            XLWorkbook workbook = new(xlsxStream);
            return (true, workbook);
        }
    }
}
