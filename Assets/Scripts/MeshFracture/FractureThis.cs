using UnityEngine;
using Random = System.Random;

namespace Project.Scripts.Fractures
{
    public class FractureThis : MonoBehaviour
    {
        [SerializeField] private Anchor anchor = Anchor.Bottom; // 분해 과정 중 고정될 앵커 지점
        [SerializeField] private int chunks = 10;              // 분해될 조각 수
        [SerializeField] private float density = 1;            // 분해된 조각들의 밀도
        [SerializeField] private float internalStrength = 100;  // 내부 결합 강도 (클수록 잘 안부서짐)
            
        [SerializeField] private Material insideMaterial;       // 부서졌을 때 내부 매터리얼
        [SerializeField] private Material outsideMaterial;      // 부서졌을 때 외부 매터리얼

        [SerializeField] bool isAlone;

        private Random rng = new Random();  // 무작위 분해를 위해 사용

        private void Awake()
        {
            Material[] materials = GetComponent<Renderer>().sharedMaterials;

            if(outsideMaterial == null ) 
            {
                outsideMaterial = materials[0];
            }

            if( insideMaterial == null )
            {
                if(materials.Length > 1)
                    insideMaterial = materials[1];
                else
                    insideMaterial = outsideMaterial;
            }
        }

        private void Start()
        {
            FractureGameobject();
            gameObject.SetActive(false);
            //Destroy(gameObject);
        }

        public ChunkGraphManager FractureGameobject()
        {
            var seed = rng.Next();
            return Fracture.FractureGameObject(
                gameObject,
                anchor,
                seed,
                chunks,
                insideMaterial,
                outsideMaterial,
                internalStrength,
                density,
                isAlone
            );
        }
    }
}