namespace Assets.Scripts.UI
{
    public class CommonWindow : BaseInterface
    {
        public static CommonWindow Instance;

        public void Awake()
        {
            Instance = this;
        }
    }
}