using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;

    [SerializeField] private float spawnTime = 10f;
    [SerializeField] private float spawnMinDistance = 5f;
    [SerializeField] private float spawnMaxDistance = 10f;

    Transform player;
    float time = 0;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;

        if(time > spawnTime)
        {
            Spawn();
            time = 0;
        }
    }

    void Spawn()
    {
        // �����_���ȕ����𐶐� (XZ����)
        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        // �����_���ȋ����𐶐�
        float randomDistance = Random.Range(spawnMinDistance, spawnMaxDistance);

        // �I�t�Z�b�g���v�Z
        Vector3 spawnPosition = new Vector3(
            randomDirection.x * randomDistance,
            0, // �����͕K�v�ɉ����Ē���
            randomDirection.y * randomDistance
        );

        // �v���C���[�̈ʒu����ɔz�u
        spawnPosition += player.position;

        // �I�u�W�F�N�g�𐶐�
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }
}
