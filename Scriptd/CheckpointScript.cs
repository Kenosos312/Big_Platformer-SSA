using Mono.Cecil.Cil;
using UnityEditor.Build;
using UnityEngine;

public class CheckpointScript : MonoBehaviour
{
    public int CheckpointNumber;

    
    private GameObject Player;
    private void SetCheckpointPlayer(){
        PlayerMovement PlayerScript = Player.GetComponent<PlayerMovement>();
        PlayerScript.GetCheckpoint(CheckpointNumber,transform.localPosition);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player"){
            Player = collision.gameObject;
            SetCheckpointPlayer();
        }

    }

}
