using UnityEngine;
using System.Collections;

public class example : MonoBehaviour {
    IEnumerator Yield() {
        print("Do now");
        yield return new WaitForSeconds(2);
        print("Do 2 seconds later");
    }
    IEnumerator Start() {
        print("Also after 2 seconds");
        yield return new WaitForSeconds(2);
        print("This is after the Do coroutine has finished execution");
    }
}