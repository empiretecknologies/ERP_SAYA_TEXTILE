using Empire_ERP.Core.Entities;
using System.Data;

namespace Empire_ERP.Core.Interfaces
{
    public interface IItemMeasurementService
    {
        MyHttpResponseMessage QuickSearch(Common common);
        MyHttpResponseMessage Save(List<ItemMeasurement> model, Common common);
        MyHttpResponseMessage GetItemMeasurementByCode(int code, Common common);
        MyHttpResponseMessage GetItemMeasurementDetailsByCode(int code, Common common);
        MyHttpResponseMessage Delete(int code, Common common);
        MyHttpResponseMessage CopyRecord(CopyRecord code, Common common);
        MyHttpResponseMessage DeleteItemMeasurementDetailByCode(int tranID, int code, Common common);
        MyHttpResponseMessage GetDataForReport(ItemMeasurementRDLCReport modelRecord, DataTable details, Common common);
    }
}
