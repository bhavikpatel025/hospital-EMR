import { ComponentFixture, TestBed } from '@angular/core/testing';

import { QuickAppointmentDialogComponent } from './quick-appointment-dialog.component';

describe('QuickAppointmentDialogComponent', () => {
  let component: QuickAppointmentDialogComponent;
  let fixture: ComponentFixture<QuickAppointmentDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [QuickAppointmentDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(QuickAppointmentDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
