#Argos

VNC Server &amp; Client based on WebSocket

###Feature
* RFB Server on WebSocket Server
  * 웹소켓 서버는 [SuperWebSocket](https://superwebsocket.codeplex.com/)의 구현을 사용
  * 웹소켓 서버 상에 RFB 프로토콜을 구현
  * .Net Framework를 기반으로 하여 Windows 플랫폼에서만 구동 가능
* RFB Client
  * HTML5 WebSocket과 Canvas가 지원되는 모든 브라우저에서 사용 가능
  * 브라우저에서 직접 서버로 접속
* RFB Protocol
  * RFB 프로토콜의 핸드쉐이크 지원
  * 지원 가능한 인증 방식
    * VNC Authorization with DES
  * 지원 가능한 인코딩 유형
    * Only RAW(but Base64 Image)
  * Framebuffer
    * FramebufferUpdate만 지원
    * FramebufferUpdateRequest는 구현하지 않음(only server to client)
    * Base64 인코딩 사용

###Known Issue
1. 다중 디스플레이 환경에서 브라우저가 디스플레이를 이동하게 되면 framebuffer를 더 이상 렌더링하지 못하는 경우가 종종 발생
2. 윈도우 작업 관리자는 클릭할 수 없음.
3. 윈도우 작업 관리자가 활성화되어 있는 경우, PointerEvent는 동작하지 않음.
 
