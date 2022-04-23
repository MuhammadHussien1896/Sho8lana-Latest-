namespace Sho8lana.Models.ViewModels
{
    public class ContractViewModel
    {
        public ContractViewModel()
        {
            PendingContracts = new List<Contract>();
            ActiveContracts = new List<Contract>();
            DoneContracts = new List<Contract>();
        }
        public ICollection<Contract> PendingContracts { get; set; }
        public ICollection<Contract> ActiveContracts { get; set; }
        public ICollection<Contract> DoneContracts { get; set; }
    }
}
