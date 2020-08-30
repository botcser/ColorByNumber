namespace Assets.Scripts.UI
{
    public class MainInterface : BaseInterface
    {
        public static MainInterface Instance;

        public void Awake()
        {
            Instance = this;
        }
    }
}