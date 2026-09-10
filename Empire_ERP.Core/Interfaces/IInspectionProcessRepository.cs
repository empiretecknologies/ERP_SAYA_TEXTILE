using Empire_ERP.Core.Entities;
using System.Data;

namespace Empire_ERP.Core.Interfaces
{
    public interface IInspectionProcessRepository
    {
        MyHttpResponseMessage QuickSearch(Common common, Menu menu);
        MyHttpResponseMessage GetAQLChart();
        MyHttpResponseMessage Save(CustomInspectionProcess model, Common common);
        MyHttpResponseMessage SaveInspectionSheet(CustomInspectionSheet model, Common common);
        MyHttpResponseMessage GetInspectionProcessByCode(int code, Common common, Menu menu);
        MyHttpResponseMessage GetInspectionImagesByCode(int ptranId, int jobNo, bool isSpecs, Common common, Menu menu);
        MyHttpResponseMessage GetInspectionProcessDetailByCode(int code, Common common, Menu menu);
        MyHttpResponseMessage GetInspectionSheetByCode(int code, Common common, Menu menu);
        MyHttpResponseMessage GetLockGridDataByCode(int code, Common common, Menu menu);
        MyHttpResponseMessage GetBatchDetailByProcess(string process, Common common);
        MyHttpResponseMessage Delete(int code, Common common, Menu menu);
        MyHttpResponseMessage DeleteImage(DeleteImageRequest model, Common common, Menu menu);
        MyHttpResponseMessage CopyRecord(CopyRecord record, Common common, Menu menu);
        MyHttpResponseMessage DeleteInspectionProcessDetailByCode(int code, Common common, Menu menu);
        MyHttpResponseMessage GetDataForReport(InspectionProcessRDLCReport modelRecord, DataTable details, CustomMenuDetail menuDetails, Company currentCompany, Common common);
        MyHttpResponseMessage SaveInspectionImages(InspectionImageRequest reqModel, Common common);
    }
}