using Empire_ERP.Core.Entities;
using System.Data;

namespace Empire_ERP.Core.Interfaces
{
    public interface IPriceQuotedService
    {
		MyHttpResponseMessage QuickSearch(Common common);
        MyHttpResponseMessage Save(CustomPriceQuoted model, Common common);
        MyHttpResponseMessage GetPriceQuotedByCode(int code, Common common);
        MyHttpResponseMessage GetPriceQuotedDetailByCode(int code, Common common);
        MyHttpResponseMessage Delete(int code, Common common);
        MyHttpResponseMessage CopyRecord(CopyRecord code, Common common);
        MyHttpResponseMessage DeletePriceQuotedDetailByCode(int code, Common common);
        MyHttpResponseMessage GetBatchDetailByProcess(PriceQuoted model, Common common);
        MyHttpResponseMessage GetDataForReport(PriceQuotedRDLCReport modelRecord, DataTable details, Common common);
    }
}