import { Component, ElementRef, Inject, ViewChild } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { HttpSettings, HttpService } from '../service/http.service';
import { Observable } from 'rxjs';
import { NgxFileDropEntry, FileSystemFileEntry, FileSystemDirectoryEntry } from 'ngx-file-drop';
import { SignalRService } from '../service/signalR.service';
import { ServerMsg } from '../class/serverMsg';
import { MetaData } from '../class/metaData';
import {MatProgressBarModule} from '@angular/material/progress-bar';


@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {
  @ViewChild('fileInput', { static: false }) fileInput!: ElementRef;

  private baseUrl: string;
  public files: NgxFileDropEntry[] = [];
  public docxContent: string = "";
  public serverMsg: ServerMsg | undefined;
  public theText:string = "";
  public metaData: MetaData | undefined;
  public documentList: string[] = [];
  public isProgreeBarShow: boolean = false;
  public isEpi: boolean = false;

  constructor(http: HttpClient,
    @Inject('BASE_URL') baseUrl: string,
    private httpService: HttpService,
    private signalRService: SignalRService,) {
    this.baseUrl = baseUrl;
  }

  async ngOnInit() {
    this.metaData = { title: "", language: '', type: '', category: '', tags:'', disease: ''};

    this.signalRService.startConnection();
    this.signalRService.openDataListener();

    this.signalRService.onMessage().subscribe(async (message: ServerMsg) => {
      this.serverMsg = message;
      if (this.serverMsg.msgName == 'newFile') {
        this.theText = this.serverMsg.data;
      }
      if (this.serverMsg.msgName == 'chatGptDone') {
        this.isProgreeBarShow = false;
        this.getDocumentList();
      }
    });

   this.getDocumentList();
  }

  isExcelFile(option: string): boolean {
    return option.toLowerCase().endsWith('.xlsx') || option.toLowerCase().endsWith('.xls');
  }

  handleCheckboxChange(event: any) {
    event.preventDefault(); 
    this.isEpi = event.target.checked;
  }

  openFileExplorer() {
    this.fileInput.nativeElement.click();
  }

  onFileSelected(event: Event) {
    const inputElement = event.target as HTMLInputElement;
    if (inputElement.files && inputElement.files[0]) {
      const selectedFile = inputElement.files[0];
      // Use the selected file as needed
      console.log('Selected file: ' + selectedFile.name);
      // You can also perform other actions, such as uploading the file to a server.
    }
  }

  getDocumentList() {
    const apiUrl = this.baseUrl + "home/GetDocuments"; 
    this.httpService.xhr<any>({ url: apiUrl, method: 'GET' }).subscribe(
      (response) => {
        console.log(response);
        this.documentList = response;
      },
      (error) => {
        console.error('Error:', error);
      }
    );
  }

  getDocListMetadata() {
    this.isProgreeBarShow = true;
    const apiUrl = this.baseUrl + "home/GetDocumentMetadata/" + this.isEpi;
    this.httpService.xhr<any>({ url: apiUrl, method: 'GET' }).subscribe(
      (response) => {
      },
      (error) => {
        console.error('Error:', error);
      }
    );
  }

  getMetadata() {
    const apiUrl = this.baseUrl + "home/GetMetadata";

    this.httpService.xhr<any>({ url: apiUrl, method: 'POST', data: this.theText }).subscribe(
      (response) => {
       this.metaData = response;
      },
      (error) => {
        console.error('Error:', error);
      }
    );
  }

  public dropped(files: NgxFileDropEntry[]) {
    this.files = files;
    console.log(files);
    for (const droppedFile of files) {

      // Is it a file?
      if (droppedFile.fileEntry.isFile) {
        const fileEntry = droppedFile.fileEntry as FileSystemFileEntry;
        fileEntry.file((file: File) => {
          this.downloadFile(file);
        });
      } else {
        // It was a directory (empty directories are added, otherwise only files)
        const fileEntry = droppedFile.fileEntry as FileSystemDirectoryEntry;
        console.log(droppedFile.relativePath, fileEntry);
      }
    }
  }

  public fileOver(event: any) {
    console.log(event);
  }

  public fileLeave(event: any) {
    console.log(event);
  }

  private downloadFile(file: File) {
    const blob = new Blob([file], { type: file.type });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.style.display = 'none';
    a.href = url;
    a.download = file.name;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
  }
}

