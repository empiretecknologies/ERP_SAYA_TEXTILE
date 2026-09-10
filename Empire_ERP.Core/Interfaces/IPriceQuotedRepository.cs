using Empire_ERP.Core.Entities;
using System.Data;

namespace Empire_ERP.Core.Interfaces
{
    public interface IPriceQuotedRepository
    {
        MyHttpResponseMessage QuickSearch(Common common, Menu menu);
        MyHttpResponseMessage Save(CustomPriceQuoted model, Common common, Menu menu);
        MyHttpResponseMessage GetPriceQuotedByCode(int code, Common common, Menu menu);
        MyHttpResponseMessage GetPriceQuotedDetailByCode(int code, Common common, Menu menu);
        MyHttpResponseMessage GetBatchDetailByProcess(PriceQuoted model, Common common);
        MyHttpResponseMessage Delete(int code, Common common, Menu menu);
        MyHttpResponseMessage CopyRecord(CopyRecord record, Common common, Menu menu);
        MyHttpResponseMessage DeletePriceQuotedDetailByCode(int code, Common common, Menu menu);
        MyHttpResponseMessage GetDataForReport(PriceQuotedRDLCReport modelRecord, DataTable details, CustomMenuDetail menuDetails, Company currentCompany, Common common);
    }
}