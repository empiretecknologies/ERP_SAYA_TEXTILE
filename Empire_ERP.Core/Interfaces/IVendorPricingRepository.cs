using Empire_ERP.Core.Entities;
using System.Data;

namespace Empire_ERP.Core.Interfaces
{
    public interface IVendorPricingRepository
    {
        MyHttpResponseMessage QuickSearch(Common common, Menu menu);
        MyHttpResponseMessage Save(CustomVendorPricing model, Common common, Menu menu);
        MyHttpResponseMessage GetVendorPricingByCode(int code, Common common, Menu menu);
        MyHttpResponseMessage GetVendorPricingDetailByCode(int code, Common common, Menu menu);
        MyHttpResponseMessage GetBatchDetailByProcess(VendorPricing model, Common common);
        MyHttpResponseMessage Delete(int code, Common common, Menu menu);
        MyHttpResponseMessage CopyRecord(CopyRecord record, Common common, Menu menu);
        MyHttpResponseMessage DeleteVendorPricingDetailByCode(int code, Common common, Menu menu);
        MyHttpResponseMessage GetDataForReport(VendorPricingRDLCReport modelRecord, DataTable details, CustomMenuDetail menuDetails, Company currentCompany, Common common);
    }
}