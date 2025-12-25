using UnityEngine;

namespace TooManyItems.Handlers
{
    public class PermafrostRotationHandler : MonoBehaviour
    {
        Transform[] wispShells = new Transform[4];
        // Start is called before the first frame update
        void Start()
        {
            wispShells[0] = transform.Find("Wisp3");
            wispShells[1] = transform.Find("Wisp4");
        }

        // Update is called once per frame
        void Update()
        {
            for (int i = 0; i < 2; i++)
            {
                if (wispShells[i])
                    wispShells[i].Rotate(0, 0, 0.08f * Mathf.Pow(-1, i));
            }
        }
    }

}
