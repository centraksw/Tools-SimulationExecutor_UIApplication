import { ChangeDetectorRef, Component, ElementRef, HostListener, OnInit, ViewChild, } from '@angular/core';
import { MatFormFieldControl } from '@angular/material/form-field';
import { FormBuilder, FormControl } from "@angular/forms";
import { SimulationDataService } from './simulation-data.service';
import { Subscription, filter } from 'rxjs';
import { DatePipe } from '@angular/common';

interface Quote {
  tagId: string;
  time: string;
  tagType: string;
  starId: number;
  roomId: number;
  rssi: number;
  keys: number;
  x: number;
  y: number;
  campus: string;
  building: string;
  floor: string;
  packetTime: Date;
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

  title = 'Simulation_Executor_Receiver';
  private maxQueueSize = 4000;
  Responsequotes: any[] = [];
  Filteredquotes: any[] = [];
  connectionStatus: string = 'Disconnected.';
  WarningMsg: string =  '';  
  filterTagId: string = '';
  isFilterApplied: boolean = false;
  isAutoScrollEnabled: boolean = false;
  isChecked = false;
  currentYear = new Date().getFullYear()
  @ViewChild('tableContainer', { static: false }) tableContainer!: ElementRef;

  FilterStatusString: string = 'Filter';
  control = new FormControl();
  sub!: Subscription;
  stockQuote!: any;
  filterTags: string[] = []; 
  private intervalId: any;

  constructor(public fb: FormBuilder, private dataService: SimulationDataService, private cdr: ChangeDetectorRef, private datePipe: DatePipe) { }

  ngOnInit() {

    /*Connect the redis and observe the emitted message on redisMessage from Simulation_Executor_API*/

    this.dataService.connectionStatus$.subscribe(status => {
      this.connectionStatus = status;
      this.cdr.detectChanges();
    });

    /*subscribe the message from redisMessage*/
    this.sub = this.dataService.getQuotes().subscribe(quote => {
      const currentDate = new Date();
      const currentDateTime = this.datePipe.transform(currentDate, 'yyyy-MM-dd HH:mm:ss')!;
      const parsedResponse = JSON.parse(quote);
      const updatedResponse = {
        ...parsedResponse,   // Spread the existing properties of the response
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
  

  /*Filter algorithm*/

  applyFilter(): void {

    if (this.filterTagId == "") {
      this.WarningMsg = 'Please provide a Tag Id to apply the filter.';
      return; 
    } 

    this.isFilterApplied = true;     //flag to control whether filter status is clicked or not  

    this.filterTags = this.filterTagId.split(',').map(tagId => tagId.trim().toLowerCase());  

    this.UpdatefilteredResponsequotes();
   
    this.cdr.detectChanges(); 
}


  UpdatefilteredResponsequotes() {  

   let accumulatedFilteredResults: Quote[] = [];
          
   for (let i = 0; i < this.filterTags.length; i++) {
     
     const filterTag = this.filterTags[i];

     if (filterTag != "") {
       
      const filteredResult = this.Responsequotes.filter(item => {
       return String(item.tagId) === filterTag;
       });
       
       accumulatedFilteredResults = [...accumulatedFilteredResults, ...filteredResult];//to add the newly added tag filteredResult to updated accumulatedFilteredResults array            
       this.Filteredquotes = this.sortResults(accumulatedFilteredResults);

       if(this.isFilterApplied && this.Responsequotes.length > 0){
        if (accumulatedFilteredResults.length === 0) {
          this.WarningMsg = 'No results found for the provided Tag Id. Please verify and try again.';          
        } 
      }
     }  
    }
  }



  sortResults(results: Quote[]): Quote[] {

    return results.sort((a, b) => {
      return String(a.time).localeCompare(String(b.time)); // Sort by time, or adjust sorting logic as needed
    });
  }

  onKeyDown(event: KeyboardEvent): void {   /*Filter 8 backspace , 46 delete , 13 enter control*/

    if (event.keyCode === 8 || event.keyCode === 46 ) {   
      
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
    this.cdr.detectChanges();     
  }
  
  CloseWarningMsg():void{  

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

