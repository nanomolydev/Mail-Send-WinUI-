using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mail_Send.ViewModels;
internal class home_model
{
    public localizated localiz
    {
        get; set;
    }
    public ProgressViewModel progress
    {
        get; set;
    }

    public home_model()
    {
        localiz = new localizated();
        progress = new ProgressViewModel();
    }
}
