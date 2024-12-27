import { ChangeDetectorRef, Component, ElementRef, HostListener, OnInit, ViewChild, } from '@angular/core';
import { MatFormFieldControl } from '@angular/material/form-field';
import { FormBuilder, FormControl } from "@angular/forms";
import { SimulationDataService } from './simulation-data.service';
import { Subscription, filter } from 'rxjs';
import { DatePipe } from '@angular/common';

// interface Quote {
//   time : string;
//   tagType : string;
//   command : string;
//   rssi : string;
//   flkeys : string;
//   monitorRFId : string;
//   floorId : string;
//   campus : string;
//   building : string;
//   floor : string;
// }

interface Location {
  starId: number;
  roomId: number;
  dwellTime: number;
  rssi: number;
  keys: string[] | null;
  campus: string;
  building: string;
  floor: string;
  x: number;
  y: number;
  latitude: number | null;
  longitude: number | null;
  latency: number;
}

interface Quote {
  tagIds: string[];
  tagType: string;
  rfReportRate: number;
  wifiReportRate: number;
  bleReportRate: number;
  activeTime: number;
  rfActiveReportRate: number;
  wifiActiveReportRate: number;
  bleActiveReportRate: number;
  ReceivedTime: any;
  reloadConfigInterval: number;
  packetTime: number;
  locations: Location[];
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
  providers: [
    { provide: MatFormFieldControl, useExisting: AppComponent }
  ]
})


export class AppComponent implements OnInit {

  //IPValidator = require('ip-validator');
  private maxQueueSize = 4000;
  Responsequotes: any[] = [];
  Filteredquotes: any[] = [];
  connectionStatus: string = 'Disconnected.';
  WarningMsg: string = '';
  filterTagId: string = '';
  isFilterApplied: boolean = false;
  isAutoScrollEnabled: boolean = false;
  currentYear = new Date().getFullYear()
  base64String: string | ArrayBuffer | null = '';
  serverName: string = '';
  channel: string = 'SENDCOMMAND';
  publishedMessage: string = '';
  @ViewChild('tableContainer', { static: false }) tableContainer!: ElementRef;
  selectedFileType: string = 'CSV';
  control = new FormControl();
  sub!: Subscription;
  stockQuote!: any;
  fileAccept: string = '.csv';
  filterTags: string[] = [];
  private intervalId: any;
  FilterStatusString: string = 'Filter';

  constructor(public fb: FormBuilder, private dataService: SimulationDataService, private cdr: ChangeDetectorRef,  private datePipe: DatePipe) { }

  form = new FormBuilder().group({
    fileType: new FormControl(),
    fileName: new FormControl(),
    serverName: new FormControl()
  });

  ngOnInit() {

    this.form.patchValue({
      fileType: 'CSV'//set the default value for the file Type
    });

    /*Connect the redis and observe the emitted message on redisMessage from Simulation_Executor_API*/

    this.dataService.connectionStatus$.subscribe(status => {
      this.connectionStatus = status;
      this.cdr.detectChanges();
    });

    /*subscribe the message from redisMessage*/

    this.sub = this.dataService.getQuotes().subscribe(quote => {
      const parsedResponse = JSON.parse(quote);

      const currentDate = new Date();
      const currentDateTime = this.datePipe.transform(currentDate, 'yyyy-MM-dd HH:mm:ss')!;
      const updatedResponse = {
        ...parsedResponse,   
        packetTime: currentDateTime,  // Add current date-time field
      };

      this.Responsequotes.push(updatedResponse);

      if (this.Responsequotes.length > this.maxQueueSize) {
        this.Responsequotes.shift(); // Remove the first item when it reaches the maxQueueSize (FIFO)
      }


      this.UpdateListViewInterval()

    });

  }

  UpdateListViewInterval(): void {

    this.intervalId = setInterval(() => {
      this.UpdatefilteredResponsequotes();
    }, 1000);

    this.cdr.detectChanges(); // This ensures the UI is updated immediately   
  }

  clearInterval(): void {
    if (this.intervalId) {
      clearInterval(this.intervalId);
    }
  }

  ngOnDestroy() {

    this.sub.unsubscribe();
  }

  /*Form */

  onSelected(value: string): void {

    this.selectedFileType = value;

    if (this.selectedFileType === 'JSON') {
      this.fileAccept = 'application/json';
    }
    else if (this.selectedFileType === 'CSV') {
      this.fileAccept = '.csv';
    }

  }

  onFileSelected(event: Event): void {

    /* send a command to Device simulator*/
    const input = event.target as HTMLInputElement;

    if (input.files && input.files.length > 0) {

      const file = input.files[0];
      const fName = file.name.toLowerCase();

      if (fName.includes(this.selectedFileType.toLowerCase())) {

        this.convertFileToBase64(file);
      }
      else {

        this.WarningMsg = 'The selected file type "' + this.selectedFileType + '" is not compatible. Please choose a valid file type.';
        this.form.patchValue({
          fileName: ''//empty the file name          
        });
        //this.form.value.fileName = ""; doesnt update in ui
      }

    }
  }

  onSendCommand(): void {


    if (this.form.value.fileName && this.form.value.fileType && this.form.value.serverName) {

      if (!this.validateIP(this.form.value.serverName)) {
        this.WarningMsg = 'Invalid IP address. Ensure the format is correct';
      }
      else
        this.PrepareJsonData()
    }
    else {

      if (!this.form.value.fileName && !this.form.value.serverName)
        this.WarningMsg = 'CSV/JSON file and IP Address are required to proceed.';

      else if (!this.form.value.fileName || !this.form.value.fileType)
        this.WarningMsg = 'Please select the file to parse';

      else if (!this.form.value.serverName)
        this.WarningMsg = 'Please provide the Server Name or IP Address.';
    }
  }

  validateIP(ip: string): boolean {

    const ipv4Regex = /^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$/;
    const ipv6Regex = /^([0-9a-fA-F]{1,4}:){7}([0-9a-fA-F]{1,4})$/;

    return ipv4Regex.test(ip) || ipv6Regex.test(ip);
  }

  PrepareJsonData() {

    this.selectedFileType = this.form.value.fileType;
    this.serverName = this.form.value.serverName;

    const commandData = {
      fileType: this.selectedFileType,
      file: this.base64String,
      serverName: this.serverName
    };

    this.publishedMessage = JSON.stringify(commandData);
    this.publish();
    this.WarningMsg = 'Data published to the Ip address ' + this.serverName;
  }

  convertFileToBase64(file: File): void {
    const reader = new FileReader();
    reader.onload = () => {
      this.base64String = reader.result;
      console.log('Base64 String:', this.base64String);
    };
    reader.readAsDataURL(file);
  }

  publish() {
    this.dataService.publishMessage(this.channel, this.publishedMessage).subscribe(response => {
    });
  }

  /*Filter algorithm*/

  applyFilter(): void {

    if (this.filterTagId == "") {
      this.WarningMsg = 'Please provide a Tag Id to apply the filter.';
      return;
    }

    this.isFilterApplied = true;     //flag to control whether filter status is clicked or not      

    this.filterTags = this.filterTagId.split(',').map(tagId => tagId.trim().toLowerCase());

    this.FilterStatusString = "Filtering";

    this.UpdatefilteredResponsequotes();

    this.cdr.detectChanges();
  }


  UpdatefilteredResponsequotes() {

    let accumulatedFilteredResults: Quote[] = [];
    console.log("logg"+this.filterTags.length);

    for (let i = 0; i < this.filterTags.length; i++) {

      const filterTag = this.filterTags[i];

      if (filterTag != "") {

        const filteredResult = this.Responsequotes.filter(item => {
          return String(item.tagId) === filterTag;
        });

        accumulatedFilteredResults = [...accumulatedFilteredResults, ...filteredResult];//to add the newly added tag filteredResult to updated accumulatedFilteredResults array            
        this.Filteredquotes = this.sortResults(accumulatedFilteredResults);
        console.log("Data>>> " + this.Filteredquotes);
        if (this.isFilterApplied && this.Responsequotes.length > 0) {
          if (accumulatedFilteredResults.length === 0) {
            this.WarningMsg = 'No results found for the provided Tag Id. Please verify and try again.';
          }
        }
      }
    }
  }

  sortResults(results: Quote[]): Quote[] {

    return results.sort((a, b) => {
      return String(a.ReceivedTime).localeCompare(String(b.ReceivedTime)); // Sort by time, or adjust sorting logic as needed
    });
  }

  onKeyDown(event: KeyboardEvent): void {   /*Filter 8 backspace , 46 delete , 13 enter control*/

    if (event.keyCode === 8 || event.keyCode === 46) {

      //this.UpdatefilteredResponsequotes;
    }
  }

  @HostListener('document:keydown', ['$event'])  //to listen the enter event 
  onKeyEnter_Filter(event: KeyboardEvent): void {
    //13 - enter key's keycode
    if (event.keyCode === 13) {
      if (this.filterTagId != "")
        this.applyFilter();
    }
  }

  clearFilter(): void {

    this.filterTagId = '';
    this.isFilterApplied = false;
    this.FilterStatusString = "Filter";
    this.UpdatefilteredResponsequotes;
    this.cdr.detectChanges();
  }

  CloseWarningMsg(): void {

    this.clearFilter();
    this.WarningMsg = ''
  }

  /*CSV*/

  OnExportCSV(): void {

    let csvData: string;
    const currentDate = new Date();
    const month = currentDate.getMonth() + 1; // getMonth() returns 0-11, so we add 1
    const day = currentDate.getDate();
    const year = currentDate.getFullYear();

    const formattedDate = `${month < 10 ? '0' + month : month}_${day < 10 ? '0' + day : day}_${year}`;//add 0 when the month or date less than 0

    if (this.isFilterApplied)
      csvData = this.convertToCSV(this.Filteredquotes);
    else
      csvData = this.convertToCSV(this.Responsequotes);

    const blob = new Blob([csvData], { type: 'text/csv' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = 'ResponseLog_' + formattedDate + '.csv';
    link.click();
  }

  convertToCSV(data: any[]): string {

    const headers = Object.keys(data[0]);

    const rows = data.map(row => {
      return headers.map(fieldName => JSON.stringify(row[fieldName] || '')).join(',');
    });

    return [headers.join(','), ...rows].join('\n');
  }

  /*List view clear*/

  OnListViewClear(): void {

    this.Filteredquotes = [];
    this.Responsequotes = [];
    this.cdr.detectChanges();
  }

  /*Auto scroll*/

  toggleAutoScroll(event: Event): void {
    this.isAutoScrollEnabled = (event.target as HTMLInputElement).checked;
  }

  // Scroll to the bottom if auto-scroll is enabled
  private scrollToBottom() {

    if (this.tableContainer) {
      const container = this.tableContainer.nativeElement;
      container.scrollTop = container.scrollHeight;
    }
  }

  // Check auto-scroll status after each change in view
  ngAfterViewChecked(): void {

    if (this.isAutoScrollEnabled && this.tableContainer) {
      this.scrollToBottom();
    }
  }

}

