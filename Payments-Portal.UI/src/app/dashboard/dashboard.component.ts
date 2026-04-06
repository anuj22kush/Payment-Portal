import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PaymentsService } from '../payments.service';
import { PaymentDto } from '../models/payment.model';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  payments: PaymentDto[] = [];
  loading = false;

  constructor(private paymentsService: PaymentsService) { }

  ngOnInit(): void {
    this.loadPayments();
  }

  loadPayments() {
    this.loading = true;
    this.paymentsService.getPayments().subscribe({
      next: (data) => {
        this.payments = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error fetching payments', err);
        this.loading = false;
      }
    });
  }

  deletePayment(id: string) {
    if (confirm('Are you sure you want to delete this payment?')) {
      this.paymentsService.deletePayment(id).subscribe(() => {
        this.loadPayments();
      });
    }
  }
}
