using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ScadaCoreWCF.serviceContracts
{
    [ServiceContract(CallbackContract = typeof(ITrendingCallback), SessionMode = SessionMode.Required)]
    public interface ITrending
    {
        [OperationContract]
        string TrendingConnection();

    }

    public interface ITrendingCallback
    {
        [OperationContract(IsOneWay = true)]
        void ITagValueChanged(string message);
    }
}
