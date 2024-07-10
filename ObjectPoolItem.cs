
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public enum ActiveMode{
    Undefined,Active,Deactive
}

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class ObjectPoolItem : UdonSharpBehaviour
{
    [SerializeField]SkinnedMeshRenderer[] skinnedMeshRenderers;
    [SerializeField]MeshRenderer[] meshRenderers;
    [SerializeField]Rigidbody[] rigidbodies;
    [SerializeField]CharacterController characterController;
    [SerializeField]BoxCollider[] boxColliders;
    [SerializeField]CapsuleCollider[] capsuleColliders;
    [SerializeField]SphereCollider[] sphereColliders;

    [SerializeField]ParticleSystem[] particleSystems;

    [SerializeField]UdonSharpBehaviour[] callbacks;

    [SerializeField]SpriteRenderer[] spriteRenderers;
    [SerializeField]AudioSource audioSource;
    [SerializeField]LineRenderer lineRenderer;
    [SerializeField]Animator animator;

    [SerializeField]bool useInitialPos=true;
    //ool isInit=false;

    Vector3 initialPos;

    public ActiveMode ActiveMode=>activeMode;

    ActiveMode activeMode=ActiveMode.Undefined;

    public bool SetActive(bool active,bool forceChange=false){
        if(!forceChange){
            if(activeMode==ActiveMode.Active && active)return false;
            if(activeMode==ActiveMode.Deactive && !active)return false;
        }
        activeMode = active?ActiveMode.Active:ActiveMode.Deactive;
        //if(/*isInit &&*/ isActive==active)return;
        //isInit=true;
        if(active && useInitialPos){
            transform.position=initialPos;
        }
        Toggle(active);
        return true;
    }

    void Toggle(bool active){
        if(skinnedMeshRenderers!=null){
            for(int i=0;i<skinnedMeshRenderers.Length;i++){
                skinnedMeshRenderers[i].enabled=active;
            }
        }
        if(meshRenderers!=null){
            for(int i=0;i<meshRenderers.Length;i++){
                meshRenderers[i].enabled=active;
            }
        }
        if(rigidbodies!=null){
            for(int i=0;i<rigidbodies.Length;i++){
                rigidbodies[i].isKinematic=!active;
            }
        }
        if(boxColliders!=null){
            for(int i=0;i<boxColliders.Length;i++){
                boxColliders[i].enabled=active;
            }
        }
        if(capsuleColliders!=null){
            for(int i=0;i<capsuleColliders.Length;i++){
                capsuleColliders[i].enabled=active;
            }
        }
        if(sphereColliders!=null){
            for(int i=0;i<sphereColliders.Length;i++){
                sphereColliders[i].enabled=active;
            }
        }
        if(particleSystems!=null){
            for(int i=0;i<particleSystems.Length;i++){
                if(active){
                    particleSystems[i].Play();
                }else{
                    particleSystems[i].Stop();
                }
            }
        }
        if(characterController!=null){
            characterController.enabled=active;
        }
        if(spriteRenderers!=null){
            for(int i=0;i<spriteRenderers.Length;i++){
                spriteRenderers[i].enabled=active;
            }
        }
        if(audioSource!=null){
            if(active){
                audioSource.Stop();
                audioSource.Play();
            } else {
                audioSource.Stop();
            }
        }
        if(lineRenderer!=null){
            lineRenderer.enabled=active;
        }
        if(animator!=null){
            animator.enabled=active;
        }
        if(callbacks!=null){
            for(int i=0;i<callbacks.Length;i++){
               
               callbacks[i].SendCustomEvent(active?"_OnEnable":"_OnDisable");
            }
        }
    }

    public bool _SetActive(bool active){
        if(activeMode==ActiveMode.Active && active)return false;
        if(activeMode==ActiveMode.Deactive && !active)return false;
        Toggle(active);
        return true;
    }
    void Start()
    {
        initialPos=transform.position;
    }
}
