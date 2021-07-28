let host = window.location.host;
let ws = new WebSocket('wss://'+host+'/ws');
let chatBox = document.getElementById("chat");


ws.onopen = function (event) {}
ws.onmessage = function (event) {
    let text = event.data;
    try{
        const data = JSON.parse(event.data)
        data.forEach(function(item, i, arr) {
            if (item.hasOwnProperty("Id")
                &&item.hasOwnProperty("Ip")
                &&item.hasOwnProperty("Message")){
                text = item["Message"];
                let messageBox = document.createElement("p");
                messageBox.innerText = text;
                chatBox.appendChild(messageBox);
            }
        });
        
    }catch (e)
    {
        let messageBox = document.createElement("p");
        messageBox.innerText = text;
        chatBox.appendChild(messageBox)
    }
};

let sendButton = document.getElementById("send");
sendButton.onclick = function (event) {
    let message = document.getElementById("text_field").value
    
    ws.send(message)
    
    let messageBox = document.createElement("p");
    
    messageBox.innerText = message;
    chatBox.appendChild(messageBox)
};