using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction.Triggers
{
    public class LogicTrigger : Trigger
    {
        public enum LogicMode
		{
            And,
            Or,
            Not // not inverts an or trigger
		}

        [SerializeField] LogicMode mode;
        [SerializeField] Trigger[] triggers;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            bool value = false;

            switch (mode)
			{
				case LogicMode.And:
                    value = true;

                    for(int i=0; i < triggers.Length; i++)
					{
                        if (triggers[i].IsActive)
                        {
                            value = false;
                            break;
                        }
					}

					break;

				case LogicMode.Or:
                    value = false;

                    for(int i=0; i < triggers.Length; i++)
					{
                        if(triggers[i].IsActive)
						{
                            value = true;
                            break;
						}
					}

					break;

				case LogicMode.Not:
                    value = false;

                    for (int i = 0; i < triggers.Length; i++)
                    {
                        if (triggers[i].IsActive)
                        {
                            value = true;
                            break;
                        }
                    }

                    value = !value;
                    break;

				default:
					break;
			}

            if (value) Activate();
            else Deactivate();
        }
    }
}