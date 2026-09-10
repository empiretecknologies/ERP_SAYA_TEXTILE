using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Empire_ERP.Core.Services
{
    public class YarnService : IYarnService
    {
        public IYarnRepository _YarnRepository { get; set; }
        public YarnService(IYarnRepository YarnRepository)
        {
            _YarnRepository = YarnRepository;
        }

        public MyHttpResponseMessage QuickSearch(Common common)
        {
            return _YarnRepository.QuickSearch(common);
        }

        public MyHttpResponseMessage Save(Yarn model, Common common)
        {
            return _YarnRepository.Save(model, common);
        }

        public string GenerateNextId(Common common)
        {
            return _YarnRepository.GenerateNextId(common);
        }

        public MyHttpResponseMessage GetYarnById(int id, Common common)
        {
            return _YarnRepository.GetYarnById(id, common);
        }

        public MyHttpResponseMessage Delete(int id, Common common)
        {
            return _YarnRepository.Delete(id, common);
        }
    }
}