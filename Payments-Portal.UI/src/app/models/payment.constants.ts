/**
 * Shared payment-related constants.
 * Centralised here so currency lists stay consistent across components.
 */
export const SUPPORTED_CURRENCIES = ['USD', 'EUR', 'INR', 'GBP'] as const;

export type Currency = typeof SUPPORTED_CURRENCIES[number];
