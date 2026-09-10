using Empire_ERP.Core.Entities;
using System.Data;

namespace Empire_ERP.Core.Interfaces
{
    public interface IInspectionProcessService
    {
		MyHttpResponseMessage QuickSearch(Common common);
		MyHttpResponseMessage GetAQLChart();
        MyHttpResponseMessage Save(CustomInspectionProcess model, Common common);
        MyHttpResponseMessage SaveInspectionSheet(CustomInspectionSheet model, Common common);
        MyHttpResponseMessage GetInspectionProcessByCode(int code, Common common);
        MyHttpResponseMessage GetInspectionImagesByCode(int ptranId, int jobNo, bool isSpecs, Common common);
        MyHttpResponseMessage GetInspectionProcessDetailByCode(int code, Common common);
        MyHttpResponseMessage GetInspectionSheetByCode(int code, Common common);
        MyHttpResponseMessage GetLockGridDataByCode(int code, Common common);
        MyHttpResponseMessage Delete(int code, Common common);
        MyHttpResponseMessage DeleteImage(DeleteImageRequest model, Common common);
        MyHttpResponseMessage CopyRecord(CopyRecord code, Common common);
        MyHttpResponseMessage DeleteInspectionProcessDetailByCode(int code, Common common);
        MyHttpResponseMessage GetBatchDetailByProcess(string process, Common common);
        MyHttpResponseMessage GetDataForReport(InspectionProcessRDLCReport modelRecord, DataTable details, Common common);
        MyHttpResponseMessage SaveInspectionImages(InspectionImageRequest reqModel, Common common);
    }
}