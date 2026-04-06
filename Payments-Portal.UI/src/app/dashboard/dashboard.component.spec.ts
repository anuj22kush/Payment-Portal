import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { of, throwError, Subject } from 'rxjs';
import { RouterModule, ActivatedRoute } from '@angular/router';

import { DashboardComponent } from './dashboard.component';
import { PaymentsService } from '../payments.service';
import { PaymentDto } from '../models/payment.model';

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;
  let mockPaymentsService: jasmine.SpyObj<PaymentsService>;
  let mockActivatedRoute: any;

  const mockPayments: PaymentDto[] = [
    { id: '1', reference: 'PAY-20260405-0001', amount: 100, currency: 'USD', createdAt: '2026-04-05T10:00:00Z' },
    { id: '2', reference: 'PAY-20260405-0002', amount: 250, currency: 'EUR', createdAt: '2026-04-05T11:00:00Z' }
  ];

  beforeEach(async () => {
    mockPaymentsService = jasmine.createSpyObj('PaymentsService', ['getPayments', 'deletePayment']);
    mockPaymentsService.getPayments.and.returnValue(of(mockPayments));
    mockPaymentsService.deletePayment.and.returnValue(of(void 0));

    mockActivatedRoute = {
      snapshot: {
        data: {
          payments: { payments: mockPayments, error: null }
        }
      }
    };

    await TestBed.configureTestingModule({
      imports: [DashboardComponent, RouterModule.forRoot([])],
      providers: [
        { provide: PaymentsService, useValue: mockPaymentsService },
        { provide: ActivatedRoute, useValue: mockActivatedRoute }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load payments on init from resolver', fakeAsync(() => {
    fixture.detectChanges();
    tick();

    expect(component.payments.length).toBe(2);
    expect(component.loading).toBeFalse();
    expect(component.errorMessage).toBeNull();
  }));

  it('should display payments in the table', fakeAsync(() => {
    fixture.detectChanges();
    tick();

    const compiled = fixture.nativeElement as HTMLElement;
    const rows = compiled.querySelectorAll('tbody tr');
    expect(rows.length).toBe(2);
  }));

  it('should display payment reference in table', fakeAsync(() => {
    fixture.detectChanges();
    tick();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('PAY-20260405-0001');
    expect(compiled.textContent).toContain('PAY-20260405-0002');
  }));

  it('should show "No payments found" when list is empty', fakeAsync(() => {
    mockActivatedRoute.snapshot.data.payments = { payments: [], error: null };
    component.ngOnInit();
    fixture.detectChanges();
    tick();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('No payments found');
  }));

  it('should display error message from resolver', fakeAsync(() => {
    mockActivatedRoute.snapshot.data.payments = {
      payments: [],
      error: 'Failed to load payments. Please try again later.'
    };
    component.ngOnInit();
    fixture.detectChanges();
    tick();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Failed to load payments');
  }));

  it('should show loading indicator while fetching', fakeAsync(() => {
    // Use a Subject so the observable does NOT complete synchronously
    const subject = new Subject<PaymentDto[]>();
    mockPaymentsService.getPayments.and.returnValue(subject.asObservable());

    component.loadPayments(); // manually call loadPayments

    expect(component.loading).toBeTrue();
    const compiled = fixture.nativeElement as HTMLElement;
    fixture.detectChanges();
    expect(compiled.textContent).toContain('Loading payments...');

    // Now emit data and verify loading goes away
    subject.next(mockPayments);
    subject.complete();
    fixture.detectChanges();
    tick();
    expect(component.loading).toBeFalse();
  }));

  it('should handle error when loading payments manually', fakeAsync(() => {
    spyOn(console, 'error');
    mockPaymentsService.getPayments.and.returnValue(throwError(() => new Error('Network error')));

    component.loadPayments();
    tick();

    expect(component.loading).toBeFalse();
    expect(component.errorMessage).toBeTruthy();
  }));

  it('should call deletePayment and reload', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    
    // Reset call count after init
    mockPaymentsService.getPayments.calls.reset();

    spyOn(window, 'confirm').and.returnValue(true);
    component.deletePayment('1');
    tick();

    expect(mockPaymentsService.deletePayment).toHaveBeenCalledWith('1');
    // getPayments called once after delete via loadPayments
    expect(mockPaymentsService.getPayments).toHaveBeenCalledTimes(1);
  }));

  it('should NOT call deletePayment if user cancels confirm', fakeAsync(() => {
    fixture.detectChanges();
    tick();

    spyOn(window, 'confirm').and.returnValue(false);
    component.deletePayment('1');
    tick();

    expect(mockPaymentsService.deletePayment).not.toHaveBeenCalled();
  }));

  it('should have an "Add Payment" link', fakeAsync(() => {
    fixture.detectChanges();
    tick();

    const compiled = fixture.nativeElement as HTMLElement;
    const addLink = compiled.querySelector('a[routerLink="/new"]');
    expect(addLink).toBeTruthy();
    expect(addLink?.textContent).toContain('Add Payment');
  }));
});
