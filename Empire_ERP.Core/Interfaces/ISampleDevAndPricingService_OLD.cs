using Empire_ERP.Core.Entities;
using System.Data;

namespace Empire_ERP.Core.Interfaces
{
    public interface ISampleDevAndPricingService_OLD
    {
        //MyHttpResponseMessage GetDeliveryFormats(Common common);
        //MyHttpResponseMessage GetAccountsForTreeView(Common common);
        MyHttpResponseMessage QuickSearch(Common common);
        //MyHttpResponseMessage QuickSearch(Common common, int skip = 0, int take = 12, string filter = null, string group = null);
        MyHttpResponseMessage GetSampleDevAndPricingById(int id, Common common);
        MyHttpResponseMessage Save(SampleDevAndPricing_OLD model, Common common);
        string GenerateNextId(Common common);
        //string GenerateGCode(string ParentId, Common common);
        MyHttpResponseMessage Delete(int id, Common common);
        MyHttpResponseMessage CopyRecord(CopyRecord code, Common common);
        MyHttpResponseMessage GetDataForReport(SampleDevAndPricingReport modelRecord, Common common);
    }
}
