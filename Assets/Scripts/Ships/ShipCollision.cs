using UnityEngine;

public class ShipCollision: MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Placed")
        {
            ShipSortingScene.Landing = false;
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.transform.tag == "Placed")
        {
            ShipSortingScene.Landing = false;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.tag == "Placed")
        {
            ShipSortingScene.Landing = true;
        }
    }
}
