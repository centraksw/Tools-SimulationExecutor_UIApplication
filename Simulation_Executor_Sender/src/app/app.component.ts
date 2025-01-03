import { Component } from '@angular/core';
import { MatFormFieldControl } from '@angular/material/form-field';
import { FormBuilder, Validators, FormGroup, FormControl } from "@angular/forms";
import { SimulationDataService } from './simulation-data.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
  providers: [
    { provide: MatFormFieldControl, useExisting: AppComponent }   
  ]
})
export class AppComponent {
  data: any;
  title = 'Simulation_Executor';
  currentDate = new Date().getFullYear()
  Types = [{
    id: 1, name: 'CSV'
  },
  {
    id: 2, name: 'JSON'
  }
];

  control = new FormControl();
  constructor(public fb: FormBuilder,private dataService: SimulationDataService) { }

  form = new FormGroup({
    fileType: new FormControl()
  });

  onSendCommand() : void
  {
    this.getData()
  }

  getData() {  
    console.log("getData")
    this.dataService.getData().subscribe(response => {
      this.data = response;
      console.log(this.data.message);
   
   // Assuming the response contains an 'items' array to be displayed in the list
    if (this.data && Array.isArray(this.data.items)) {
      console.log('Fetched items:', this.data.items);
    } else {
      console.log('No items found in the response.');
    }
    });
  } 
}
