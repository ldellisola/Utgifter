using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;


namespace Utgifter.Api.Extensions;

public static class ExcelExtensions
{
    public static void ToXlsx(this HSSFWorkbook source, MemoryStream destinationStream)
    {
        var destination = new XSSFWorkbook();
        
        try { destination.MissingCellPolicy = source.MissingCellPolicy; }
        catch
        {
            // ignored
        }

        for (int i = 0; i < source.NumberOfSheets; i++)
        {
            ISheet sSheet = source.GetSheetAt(i);
            ISheet dSheet = destination.CreateSheet(sSheet.SheetName);

            try { dSheet.ForceFormulaRecalculation = sSheet.ForceFormulaRecalculation; }
            catch
            {
                // ignored
            }

            ConvertSheet(sSheet, dSheet);

            // SheetState state = SheetState.Visible;
            //
            // try
            // {
            //     if (source.IsSheetHidden(i))
            //         state = SheetState.Hidden;
            //     if (source.IsSheetVeryHidden(i))
            //         state = SheetState.VeryHidden;
            //
            //     destination.SetSheetHidden(i, state);
            // }
            // catch
            // {
            //     // ignored
            // }
        }

        try { destination.SetActiveSheet(source.ActiveSheetIndex); }
        catch
        {
            // ignored
        }

        destination.Write(destinationStream,true);
    }
    
    private static void ConvertSheet(ISheet sSheet, ISheet dSheet)
    {
        int numberOfColumns = 0;

        for(int i = sSheet.FirstRowNum; i <= sSheet.LastRowNum; i++)
        {
            try { ConvertRow(sSheet.GetRow(i), dSheet.CreateRow(i)); }
            catch
            {
                // ignored
            }

            try
            {
                if (sSheet.GetRow(i) != null && numberOfColumns < sSheet.GetRow(i).LastCellNum)
                    numberOfColumns = sSheet.GetRow(i).LastCellNum;
            }
            catch
            {
                // ignored
            }
        }

        for(int i = 1; i <= numberOfColumns;i++)
        {
            try
            {
                if (sSheet.IsColumnHidden(i))
                    dSheet.SetColumnHidden(i, true);
            }
            catch
            {
                // ignored
            }
        }

        for (int i = 0; i < sSheet.NumMergedRegions; i++)
        {
            try { dSheet.AddMergedRegion(sSheet.GetMergedRegion(i)); }
            catch
            {
                // ignored
            }
        }
    }

    private static void ConvertRow(IRow sRow, IRow dRow)
    {
        if (sRow.FirstCellNum < 0)
            return;

        for (int i = sRow.FirstCellNum; i <= sRow.LastCellNum; i++)
        {
            if (sRow.GetCell(i) is { } cell)
                ConvertCell(cell, dRow.CreateCell(i));
        }
    }

    private static void ConvertCell(ICell sCell, ICell dCell)
    {
        try
        {
            switch (sCell.CellType)
            {
                case CellType.String:
                    dCell.SetCellValue(sCell.StringCellValue);
                    break;
                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(sCell))
                    {
                        dCell.CellStyle.DataFormat = sCell.CellStyle.DataFormat;
                        dCell.SetCellValue(sCell.DateCellValue.ToString());
                    }
                    else
                        dCell.SetCellValue(sCell.NumericCellValue);
                    break;
                case CellType.Boolean:
                    dCell.SetCellValue(sCell.BooleanCellValue);
                    break;
                case CellType.Error:
                    dCell.SetCellErrorValue(sCell.ErrorCellValue);
                    break;
                case CellType.Formula:
                    dCell.SetCellFormula(sCell.CellFormula);
                    break;
                case CellType.Unknown:
                case CellType.Blank:
                default:
                    dCell.SetCellType(CellType.Blank);
                    break;
            }
        }
        catch
        {
            // ignored
        }
    }        
}

