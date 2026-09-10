using Empire_ERP.Core.Entities;
using System.Data;

namespace Empire_ERP.Core.Interfaces
{
    public interface IVendorPricingService
    {
		MyHttpResponseMessage QuickSearch(Common common);
        MyHttpResponseMessage Save(CustomVendorPricing model, Common common);
        MyHttpResponseMessage GetVendorPricingByCode(int code, Common common);
        MyHttpResponseMessage GetVendorPricingDetailByCode(int code, Common common);
        MyHttpResponseMessage Delete(int code, Common common);
        MyHttpResponseMessage CopyRecord(CopyRecord code, Common common);
        MyHttpResponseMessage DeleteVendorPricingDetailByCode(int code, Common common);
        MyHttpResponseMessage GetBatchDetailByProcess(VendorPricing model, Common common);
        MyHttpResponseMessage GetDataForReport(VendorPricingRDLCReport modelRecord, DataTable details, Common common);
    }
}