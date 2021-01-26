using FolketsOrdbokResource;

namespace svenskabot
{
    public class RuntimeData
    {
        public FolketsOrdbok FolketsOrdbok { get; private set; }
        public DagensOrd DagensOrd { get; private set; }

        public RuntimeData()
        {
            FolketsOrdbok = new FolketsOrdbok();
            DagensOrd = new DagensOrd(FolketsOrdbok);
        }
    }
}
