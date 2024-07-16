using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

/*
 애니메이션의 상태 변경은 서버에서 처리되고 클라이언트에게 전송된다.
그리고 오너 권한이 필요하다. 본인의 애니메이션 상태를 제어하고 서버와 다른 클라이언트에게 알리기 위해서다.
이를 통해 로컬 플레이어는 애니메이션 상태 변화를 즉각적으로 확인할 수 있고, 빠른 피드백을 받을 수 있다.
이를 위해 오너 권한을 위한 스크립트를 하나 더 만들어야 한다.
 */

public class OwnerNetworkAnimator : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
