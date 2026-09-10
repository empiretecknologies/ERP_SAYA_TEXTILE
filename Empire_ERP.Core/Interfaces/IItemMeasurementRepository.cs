using Empire_ERP.Core.Entities;
using System.Data;

namespace Empire_ERP.Core.Interfaces
{
    public interface IItemMeasurementRepository
    {
        MyHttpResponseMessage QuickSearch(MyHttpResponseMessage Menu, Common common);
        //MyHttpResponseMessage QuickSearch(MyHttpResponseMessage Menu, Common common, int skip = 0, int take = 12, string filter = null, string group = null, string sort = null);
        MyHttpResponseMessage Save(List<ItemMeasurement> model, Common common, Menu menu);
        MyHttpResponseMessage GetItemMeasurementByCode(int code, Common common, Menu menu);
        MyHttpResponseMessage GetItemMeasurementDetailsByCode(int code, Common common, Menu menu);
        MyHttpResponseMessage Delete(int code, Common common, Menu menu);
        MyHttpResponseMessage CopyRecord(CopyRecord record, Common common, Menu menu);
        MyHttpResponseMessage DeleteItemMeasurementDetailByCode(int tranID, int code, Common common, Menu menu);
        MyHttpResponseMessage GetDataForReport(ItemMeasurementRDLCReport modelRecord, DataTable details, CustomMenuDetail menuDetails, Company currentCompany, Common common);
    }
}
