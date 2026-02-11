using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbArea : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Luna"))
        {
            collision.GetComponent<LunaController>().Climb(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Luna"))
        {
            collision.GetComponent<LunaController>().Climb(false);
        }
    }
}
