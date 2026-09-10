using Empire_ERP.Core.Entities;
using System.Data;

namespace Empire_ERP.Core.Interfaces
{
    public interface ISampleSubmissionService
    {
		MyHttpResponseMessage QuickSearch(Common common);
        MyHttpResponseMessage Save(CustomSampleSubmission model, Common common);
        MyHttpResponseMessage GetSampleSubmissionByCode(int code, Common common);
        MyHttpResponseMessage GetSampleSubmissionDetailByCode(int code, Common common);
        MyHttpResponseMessage Delete(int code, Common common);
        MyHttpResponseMessage CopyRecord(CopyRecord code, Common common);
        MyHttpResponseMessage DeleteSampleSubmissionDetailByCode(int code, Common common);
        MyHttpResponseMessage GetBatchDetailByProcess(SampleSubmission model, Common common);
        MyHttpResponseMessage GetDataForReport(SampleSubmissionRDLCReport modelRecord, DataTable details, Common common);
    }
}