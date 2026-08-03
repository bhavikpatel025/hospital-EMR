import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PatientShellComponent } from './patient-shell.component';

describe('PatientShellComponent', () => {
  let component: PatientShellComponent;
  let fixture: ComponentFixture<PatientShellComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PatientShellComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PatientShellComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
