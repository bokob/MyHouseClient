using UnityEngine;
using Random = System.Random;

namespace Project.Scripts.Fractures
{
    public class FractureThis : MonoBehaviour
    {
        [SerializeField] private Anchor anchor = Anchor.Bottom; // ���� ���� �� ������ ��Ŀ ����
        [SerializeField] private int chunks = 10;              // ���ص� ���� ��
        [SerializeField] private float density = 1;            // ���ص� �������� �е�
        [SerializeField] private float internalStrength = 100;  // ���� ���� ���� (Ŭ���� �� �Ⱥμ���)
            
        [SerializeField] private Material insideMaterial;       // �μ����� �� ���� ���͸���
        [SerializeField] private Material outsideMaterial;      // �μ����� �� �ܺ� ���͸���

        [SerializeField] bool isAlone;

        private Random rng = new Random();  // ������ ���ظ� ���� ���

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