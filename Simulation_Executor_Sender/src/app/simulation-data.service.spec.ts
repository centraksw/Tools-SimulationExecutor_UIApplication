import { TestBed } from '@angular/core/testing';

import { SimulationDataService } from './simulation-data.service';

describe('SimulationDataService', () => {
  let service: SimulationDataService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SimulationDataService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
