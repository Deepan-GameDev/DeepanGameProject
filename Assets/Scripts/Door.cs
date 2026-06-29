using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public bool open;
    public float smooth = 1f;
    public float openAngle = -90f;
    public AudioClip openDoor;
    public AudioClip closeDoor;

    private AudioSource audioSource;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        closedRotation = transform.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
    }

    void Update()
    {
        Quaternion targetRotation = open ? openRotation : closedRotation;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * 5f * smooth);
    }

    public void Interact()
    {
        OpenDoor();
    }

    public void OpenDoor()
    {
        open = !open;

        if (audioSource != null)
        {
            AudioClip clip = open ? openDoor : closeDoor;
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}
