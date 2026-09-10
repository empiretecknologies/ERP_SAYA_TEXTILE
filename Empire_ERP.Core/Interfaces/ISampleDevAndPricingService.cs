using Empire_ERP.Core.Entities;
using System.Data;

namespace Empire_ERP.Core.Interfaces
{
    public interface ISampleDevAndPricingService
    {
        MyHttpResponseMessage QuickSearch(Common common);
        //MyHttpResponseMessage QuickSearch(Common common, int skip = 0, int take = 12, string filter = null, string group = null, string sort = null);
        MyHttpResponseMessage GetSampleDevAndPricingByCode(int code, Common common);
        MyHttpResponseMessage GetSampleDevAndPricingDetailByCode(int code, Common common);
        MyHttpResponseMessage GetPickDataBySupplier(int pCode, int actCode, Common common);
        MyHttpResponseMessage GetSampleDevAndPricingPickDetailByCode(int code, Common common);
        MyHttpResponseMessage GetSampleDevAndPricingDetailByItem(int code, int qty, Common common);
        MyHttpResponseMessage Save(CustomSampleDevAndPricing modelRecord, Common common);
        MyHttpResponseMessage Delete(int code, Common common);
        MyHttpResponseMessage CopyRecord(CopyRecord code, Common common);
        MyHttpResponseMessage DeleteSampleDevAndPricingDetailByCode(int code, Common common);
        MyHttpResponseMessage GetDataForReport(SampleDevAndPricingRDLCReport modelRecord, DataTable inspectionServiceChargesDetails, Common common);
    }
}