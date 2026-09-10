using Empire_ERP.Core.Entities;
using System.Data;

namespace Empire_ERP.Core.Interfaces
{
    public interface ISampleSubmissionRepository
    {
        MyHttpResponseMessage QuickSearch(Common common, Menu menu);
        MyHttpResponseMessage Save(CustomSampleSubmission model, Common common, Menu menu);
        MyHttpResponseMessage GetSampleSubmissionByCode(int code, Common common, Menu menu);
        MyHttpResponseMessage GetSampleSubmissionDetailByCode(int code, Common common, Menu menu);
        MyHttpResponseMessage GetBatchDetailByProcess(SampleSubmission model, Common common);
        MyHttpResponseMessage Delete(int code, Common common, Menu menu);
        MyHttpResponseMessage CopyRecord(CopyRecord record, Common common, Menu menu);
        MyHttpResponseMessage DeleteSampleSubmissionDetailByCode(int code, Common common, Menu menu);
        MyHttpResponseMessage GetDataForReport(SampleSubmissionRDLCReport modelRecord, DataTable details, CustomMenuDetail menuDetails, Company currentCompany, Common common);
    }
}