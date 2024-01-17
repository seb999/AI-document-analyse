import { EventEmitter, Injectable } from '@angular/core';
import * as signalR from "@microsoft/signalr";
import { ServerMsg } from '../class/serverMsg';

@Injectable({
    providedIn: 'root'
})
export class SignalRService {
    public data: any;
    private hubConnection: signalR.HubConnection | undefined;
    public eventMessage: EventEmitter<ServerMsg> = new EventEmitter();

    public startConnection = () => {
        this.hubConnection = new signalR.HubConnectionBuilder()
          .withUrl('https://localhost:7142/Signalr')
        //   .withUrl('Signalr')
          .withAutomaticReconnect()
          .build();

        this.hubConnection
            .start()
            .then(() => console.log('Connection started'))
            .catch(err => console.log('Error while starting SignalR connection: ' + err))
    }

    public onMessage() {
        return this.eventMessage;
    }

    public closeConnection = () => {
        this.hubConnection?.stop();
    }

    public openDataListener = () => {

        this.hubConnection?.on("newFile", (data:any) => {
            let serverMsg: ServerMsg = {
                msgName: "newFile",
                data: JSON.parse(data)
            }
            return this.eventMessage.emit(serverMsg);
        });

        this.hubConnection?.on("chatGptDone", (data:any) => {
            let serverMsg: ServerMsg = {
                msgName: "chatGptDone",
                data: JSON.parse(data)
            }
            return this.eventMessage.emit(serverMsg);
        });
    }
}
